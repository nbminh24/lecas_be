using MongoDB.Driver;
using be_lecas.Models;
using be_lecas.DTOs;
using Microsoft.Extensions.Configuration;

namespace be_lecas.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;

        public ProductRepository(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            var databaseName = configuration.GetSection("ConnectionStrings:DatabaseName").Value ?? "lecas";
            var database = client.GetDatabase(databaseName);
            _products = database.GetCollection<Product>("products");
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _products.Find(p => p.IsActive).ToListAsync();
        }

        public async Task<List<Product>> GetByCategoryAsync(string categoryId)
        {
            return await _products.Find(p => p.CategoryId == categoryId && p.IsActive).ToListAsync();
        }

        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            return await _products.Find(filter).ToListAsync();
        }

        public async Task<List<Product>> GetFilteredAsync(ProductFilterRequest filter)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            // Active products only
            filters.Add(filterBuilder.Eq(p => p.IsActive, true));

            // Category filter
            if (!string.IsNullOrEmpty(filter.CategoryId))
            {
                filters.Add(filterBuilder.Eq(p => p.CategoryId, filter.CategoryId));
            }

            // Search term
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                filters.Add(filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")));
            }

            // Price range
            if (filter.MinPrice.HasValue)
            {
                filters.Add(filterBuilder.Gte(p => p.Price, filter.MinPrice.Value));
            }
            if (filter.MaxPrice.HasValue)
            {
                filters.Add(filterBuilder.Lte(p => p.Price, filter.MaxPrice.Value));
            }

            // Stock filter
            if (filter.InStock.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.InStock, filter.InStock.Value));
            }

            var combinedFilter = filterBuilder.And(filters);
            var sort = GetSortDefinition(filter.SortBy, filter.SortOrder);
            var skip = (filter.Page - 1) * filter.PageSize;

            return await _products.Find(combinedFilter)
                .Sort(sort)
                .Skip((int)skip)
                .Limit((int)filter.PageSize)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _products.InsertOneAsync(product);
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            await _products.ReplaceOneAsync(p => p.Id == product.Id, product);
            return product;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _products.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _products.Find(p => p.Id == id).AnyAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(string productId, int limit = 10)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return new List<Product>();

            // Thuật toán tối ưu: Phân tầng để giảm tải
            // 1. Lấy sản phẩm cùng category + rating cao (top 50)
            // 2. Tính score chỉ trên subset này
            // 3. Trả về top 10

            var pipeline = new[]
            {
                new MongoDB.Bson.BsonDocument("$match", new MongoDB.Bson.BsonDocument
                {
                    { "_id", new MongoDB.Bson.BsonDocument("$ne", MongoDB.Bson.ObjectId.Parse(productId)) },
                    { "categoryId", product.CategoryId },
                    { "isActive", true },
                    { "rating", new MongoDB.Bson.BsonDocument("$gte", 3.5) } // Chỉ lấy sản phẩm rating >= 3.5
                }),
                new MongoDB.Bson.BsonDocument("$sort", new MongoDB.Bson.BsonDocument("rating", -1)),
                new MongoDB.Bson.BsonDocument("$limit", 50), // Giới hạn 50 sản phẩm để tính score
                new MongoDB.Bson.BsonDocument("$addFields", new MongoDB.Bson.BsonDocument
                {
                    { "score", new MongoDB.Bson.BsonDocument("$add", new MongoDB.Bson.BsonArray
                        {
                            new MongoDB.Bson.BsonDocument("$multiply", new MongoDB.Bson.BsonArray { "$rating", 0.5 }),
                            new MongoDB.Bson.BsonDocument("$cond", new MongoDB.Bson.BsonArray
                                {
                                    new MongoDB.Bson.BsonDocument("$eq", new MongoDB.Bson.BsonArray { "$subCategory", product.SubCategory }),
                                    0.3,
                                    0
                                }),
                            new MongoDB.Bson.BsonDocument("$cond", new MongoDB.Bson.BsonArray
                                {
                                    new MongoDB.Bson.BsonDocument("$and", new MongoDB.Bson.BsonArray
                                        {
                                            new MongoDB.Bson.BsonDocument("$gte", new MongoDB.Bson.BsonArray { "$price", (double)product.Price * 0.7 }),
                                            new MongoDB.Bson.BsonDocument("$lte", new MongoDB.Bson.BsonArray { "$price", (double)product.Price * 1.3 })
                                        }),
                                    0.2,
                                    0
                                })
                        })
                    }
                }),
                new MongoDB.Bson.BsonDocument("$sort", new MongoDB.Bson.BsonDocument("score", -1)),
                new MongoDB.Bson.BsonDocument("$limit", limit)
            };

            var results = await _products.Aggregate<Product>(pipeline).ToListAsync();
            return results;
        }

        public async Task<List<Product>> GetTopSellingAsync(int limit = 10)
        {
            return await _products.Find(p => p.IsActive)
                .SortByDescending(p => p.Rating)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<List<Product>> GetFlashSaleAsync()
        {
            // Thuật toán Flash Sale thông minh:
            // 1. Sản phẩm có giảm giá (price < originalPrice)
            // 2. Ưu tiên theo % giảm giá cao nhất
            // 3. Chỉ lấy sản phẩm còn hàng và rating tốt
            // 4. Giới hạn thời gian flash sale (nếu có)

            var pipeline = new[]
            {
                new MongoDB.Bson.BsonDocument("$match", new MongoDB.Bson.BsonDocument
                {
                    { "isActive", true },
                    { "originalPrice", new MongoDB.Bson.BsonDocument("$gt", 0) },
                    { "inStock", true },
                    { "rating", new MongoDB.Bson.BsonDocument("$gte", 3.0) },
                    { "$expr", new MongoDB.Bson.BsonDocument("$lt", new MongoDB.Bson.BsonArray { "$price", "$originalPrice" }) }
                }),
                new MongoDB.Bson.BsonDocument("$addFields", new MongoDB.Bson.BsonDocument
                {
                    { "discountPercent", new MongoDB.Bson.BsonDocument("$multiply", new MongoDB.Bson.BsonArray
                        {
                            new MongoDB.Bson.BsonDocument("$divide", new MongoDB.Bson.BsonArray
                                {
                                    new MongoDB.Bson.BsonDocument("$subtract", new MongoDB.Bson.BsonArray { "$originalPrice", "$price" }),
                                    "$originalPrice"
                                }),
                            100
                        })
                    },
                    { "flashSaleScore", new MongoDB.Bson.BsonDocument("$add", new MongoDB.Bson.BsonArray
                        {
                            new MongoDB.Bson.BsonDocument("$multiply", new MongoDB.Bson.BsonArray 
                                { 
                                    new MongoDB.Bson.BsonDocument("$divide", new MongoDB.Bson.BsonArray
                                        {
                                            new MongoDB.Bson.BsonDocument("$subtract", new MongoDB.Bson.BsonArray { "$originalPrice", "$price" }),
                                            "$originalPrice"
                                        }),
                                    0.6 // 60% trọng số cho % giảm giá
                                }),
                            new MongoDB.Bson.BsonDocument("$multiply", new MongoDB.Bson.BsonArray { "$rating", 0.3 }), // 30% cho rating
                            new MongoDB.Bson.BsonDocument("$cond", new MongoDB.Bson.BsonArray
                                {
                                    new MongoDB.Bson.BsonDocument("$gte", new MongoDB.Bson.BsonArray { "$stockQuantity", 10 }),
                                    0.1, // 10% bonus nếu còn nhiều hàng
                                    0
                                })
                        })
                    }
                }),
                new MongoDB.Bson.BsonDocument("$sort", new MongoDB.Bson.BsonDocument("flashSaleScore", -1)),
                new MongoDB.Bson.BsonDocument("$limit", 20)
            };

            var results = await _products.Aggregate<Product>(pipeline).ToListAsync();
            return results;
        }

        public async Task<int> GetTotalCountAsync(ProductFilterRequest? filter = null)
        {
            if (filter == null)
            {
                var count = await _products.CountDocumentsAsync(p => p.IsActive);
                return count > int.MaxValue ? int.MaxValue : (int)count;
            }

            var filterBuilder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>();

            filters.Add(filterBuilder.Eq(p => p.IsActive, true));

            if (!string.IsNullOrEmpty(filter.CategoryId))
            {
                filters.Add(filterBuilder.Eq(p => p.CategoryId, filter.CategoryId));
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                filters.Add(filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")));
            }

            if (filter.MinPrice.HasValue)
            {
                filters.Add(filterBuilder.Gte(p => p.Price, filter.MinPrice.Value));
            }

            if (filter.MaxPrice.HasValue)
            {
                filters.Add(filterBuilder.Lte(p => p.Price, filter.MaxPrice.Value));
            }

            if (filter.InStock.HasValue)
            {
                filters.Add(filterBuilder.Eq(p => p.InStock, filter.InStock.Value));
            }

            var combinedFilter = filterBuilder.And(filters);
            var totalCount = await _products.CountDocumentsAsync(combinedFilter);
            return totalCount > int.MaxValue ? int.MaxValue : (int)totalCount;
        }

        private SortDefinition<Product> GetSortDefinition(string? sortBy, string? sortOrder)
        {
            var sortBuilder = Builders<Product>.Sort;
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "price" => isDescending ? sortBuilder.Descending(p => p.Price) : sortBuilder.Ascending(p => p.Price),
                "name" => isDescending ? sortBuilder.Descending(p => p.Name) : sortBuilder.Ascending(p => p.Name),
                "rating" => isDescending ? sortBuilder.Descending(p => p.Rating) : sortBuilder.Ascending(p => p.Rating),
                _ => isDescending ? sortBuilder.Descending(p => p.CreatedAt) : sortBuilder.Ascending(p => p.CreatedAt)
            };
        }
    }
}

