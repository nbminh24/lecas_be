using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;

namespace be_lecas.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ProductDto>>> GetAllProductsAsync(ProductFilterRequest? filter = null)
        {
            try
            {
                var products = await _productRepository.GetFilteredAsync(filter ?? new ProductFilterRequest());
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                foreach (var productDto in productDtos)
                {
                    if (!string.IsNullOrEmpty(productDto.Category?.Id))
                    {
                        var category = await _categoryRepository.GetByIdAsync(productDto.Category.Id);
                        if (category != null)
                        {
                            productDto.Category = _mapper.Map<CategoryDto>(category);
                        }
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto?>> GetProductByIdAsync(string id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<ProductDto?>.ErrorResult("Product not found");
                }

                var productDto = _mapper.Map<ProductDto>(product);

                // Load category information
                if (!string.IsNullOrEmpty(product.CategoryId))
                {
                    var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    if (category != null)
                    {
                        productDto.Category = _mapper.Map<CategoryDto>(category);
                    }
                }

                return ApiResponse<ProductDto?>.SuccessResult(productDto, "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDto?>.ErrorResult($"Failed to get product: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> GetProductsByCategoryAsync(string categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(categoryId);
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category != null)
                {
                    var categoryDto = _mapper.Map<CategoryDto>(category);
                    foreach (var productDto in productDtos)
                    {
                        productDto.Category = categoryDto;
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get products by category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var products = await _productRepository.SearchAsync(searchTerm);
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                foreach (var productDto in productDtos)
                {
                    if (!string.IsNullOrEmpty(productDto.Category?.Id))
                    {
                        var category = await _categoryRepository.GetByIdAsync(productDto.Category.Id);
                        if (category != null)
                        {
                            productDto.Category = _mapper.Map<CategoryDto>(category);
                        }
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Search completed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to search products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                // Validate category exists
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
                if (category == null)
                {
                    return ApiResponse<ProductDto>.ErrorResult("Category not found");
                }

                var product = _mapper.Map<Product>(request);
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                product.IsActive = true;

                var createdProduct = await _productRepository.CreateAsync(product);
                var productDto = _mapper.Map<ProductDto>(createdProduct);
                productDto.Category = _mapper.Map<CategoryDto>(category);

                return ApiResponse<ProductDto>.SuccessResult(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDto>.ErrorResult($"Failed to create product: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ProductDto?>> UpdateProductAsync(string id, UpdateProductRequest request)
        {
            try
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return ApiResponse<ProductDto?>.ErrorResult("Product not found");
                }

                // Update properties
                if (!string.IsNullOrEmpty(request.Name))
                    existingProduct.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    existingProduct.Description = request.Description;

                if (request.Price.HasValue)
                    existingProduct.Price = request.Price.Value;

                if (request.OriginalPrice.HasValue)
                    existingProduct.OriginalPrice = request.OriginalPrice.Value;

                if (request.Images != null)
                    existingProduct.Images = request.Images;

                if (!string.IsNullOrEmpty(request.CategoryId))
                {
                    var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
                    if (category == null)
                    {
                        return ApiResponse<ProductDto?>.ErrorResult("Category not found");
                    }
                    existingProduct.CategoryId = request.CategoryId;
                }

                if (!string.IsNullOrEmpty(request.SubCategory))
                    existingProduct.SubCategory = request.SubCategory;

                if (request.Sizes != null)
                    existingProduct.Sizes = request.Sizes;

                if (request.Colors != null)
                    existingProduct.Colors = _mapper.Map<List<ProductColor>>(request.Colors);

                if (request.StockQuantity.HasValue)
                {
                    existingProduct.StockQuantity = request.StockQuantity.Value;
                    existingProduct.InStock = request.StockQuantity.Value > 0;
                }

                if (request.Tags != null)
                    existingProduct.Tags = request.Tags;

                existingProduct.UpdatedAt = DateTime.UtcNow;

                var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
                var productDto = _mapper.Map<ProductDto>(updatedProduct);

                // Load category information
                var categoryInfo = await _categoryRepository.GetByIdAsync(updatedProduct.CategoryId);
                if (categoryInfo != null)
                {
                    productDto.Category = _mapper.Map<CategoryDto>(categoryInfo);
                }

                return ApiResponse<ProductDto?>.SuccessResult(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProductDto?>.ErrorResult($"Failed to update product: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(string id)
        {
            try
            {
                var result = await _productRepository.DeleteAsync(id);
                if (result)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Product deleted successfully");
                }
                return ApiResponse<bool>.ErrorResult("Product not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete product: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> GetRelatedProductsAsync(string productId)
        {
            try
            {
                var products = await _productRepository.GetRelatedProductsAsync(productId);
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                foreach (var productDto in productDtos)
                {
                    if (!string.IsNullOrEmpty(productDto.Category?.Id))
                    {
                        var category = await _categoryRepository.GetByIdAsync(productDto.Category.Id);
                        if (category != null)
                        {
                            productDto.Category = _mapper.Map<CategoryDto>(category);
                        }
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Related products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get related products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> GetTopSellingProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetTopSellingAsync();
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                foreach (var productDto in productDtos)
                {
                    if (!string.IsNullOrEmpty(productDto.Category?.Id))
                    {
                        var category = await _categoryRepository.GetByIdAsync(productDto.Category.Id);
                        if (category != null)
                        {
                            productDto.Category = _mapper.Map<CategoryDto>(category);
                        }
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Top selling products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get top selling products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> GetFlashSaleProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetFlashSaleAsync();
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                foreach (var productDto in productDtos)
                {
                    if (!string.IsNullOrEmpty(productDto.Category?.Id))
                    {
                        var category = await _categoryRepository.GetByIdAsync(productDto.Category.Id);
                        if (category != null)
                        {
                            productDto.Category = _mapper.Map<CategoryDto>(category);
                        }
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Flash sale products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get flash sale products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<int>> GetTotalProductsCountAsync(ProductFilterRequest? filter = null)
        {
            try
            {
                var count = await _productRepository.GetTotalCountAsync(filter);
                return ApiResponse<int>.SuccessResult(count, "Total count retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResult($"Failed to get total count: {ex.Message}");
            }
        }
    }
}

