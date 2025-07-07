using System.ComponentModel.DataAnnotations;

namespace be_lecas.DTOs
{
    public class CartDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string SelectedSize { get; set; } = string.Empty;
        public string SelectedColor { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class AddToCartRequest
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string Size { get; set; } = string.Empty;
        [Required]
        public string Color { get; set; } = string.Empty;
    }

    public class UpdateCartItemRequest
    {
        public int? Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }

    public class CartSummaryDto
    {
        public int TotalItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }

    public class CreateCartRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<AddToCartRequest> Items { get; set; } = new List<AddToCartRequest>();
    }

    public class UpdateCartRequest
    {
        public List<UpdateCartItemRequest>? Items { get; set; }
        public decimal? Shipping { get; set; }
        public decimal? Tax { get; set; }
    }
}

