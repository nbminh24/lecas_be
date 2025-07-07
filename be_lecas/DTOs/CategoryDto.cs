using System.ComponentModel.DataAnnotations;

namespace be_lecas.DTOs
{
    public class CategoryDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CategoryDto? Parent { get; set; }
        public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();
    }

    public class CreateCategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateCategoryRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? ParentId { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
    }
} 