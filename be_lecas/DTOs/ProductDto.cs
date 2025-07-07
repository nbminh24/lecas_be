using System.ComponentModel.DataAnnotations;

namespace be_lecas.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public CategoryDto? Category { get; set; }
        public string? SubCategory { get; set; }
        public List<string> Sizes { get; set; } = new List<string>();
        public List<ProductColorDto> Colors { get; set; } = new List<ProductColorDto>();
        public bool InStock { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductColorDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool Available { get; set; }
    }

    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        [Required]
        public string CategoryId { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public List<string> Sizes { get; set; } = new List<string>();
        public List<ProductColorDto> Colors { get; set; } = new List<ProductColorDto>();
        public int StockQuantity { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public List<string>? Images { get; set; }
        public string? CategoryId { get; set; }
        public string? SubCategory { get; set; }
        public List<string>? Sizes { get; set; }
        public List<ProductColorDto>? Colors { get; set; }
        public int? StockQuantity { get; set; }
        public List<string>? Tags { get; set; }
    }

    public class ProductFilterRequest
    {
        public string? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string>? Sizes { get; set; }
        public List<string>? Colors { get; set; }
        public bool? InStock { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "createdAt";
        public string? SortOrder { get; set; } = "desc";
    }
}

