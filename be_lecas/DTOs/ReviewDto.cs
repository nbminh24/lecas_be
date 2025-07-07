using System.ComponentModel.DataAnnotations;

namespace be_lecas.DTOs
{
    public class ReviewDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? User { get; set; }
    }

    public class CreateReviewRequest
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Required]
        public string OrderId { get; set; } = string.Empty;
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class UpdateReviewRequest
    {
        [Range(1, 5)]
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? ImageUrl { get; set; }
    }
}

