using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace be_lecas.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string CategoryId { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public List<string> Sizes { get; set; } = new List<string>();
        public List<ProductColor> Colors { get; set; } = new List<ProductColor>();
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public double Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [BsonIgnore]
        public Category? Category { get; set; }
    }

    public class ProductColor
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool Available { get; set; } = true;
    }
}

