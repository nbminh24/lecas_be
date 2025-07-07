using System.ComponentModel.DataAnnotations;

namespace be_lecas.DTOs
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public ShippingInfoDto ShippingInfo { get; set; } = new ShippingInfoDto();
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public List<OrderTrackingDto> Tracking { get; set; } = new List<OrderTrackingDto>();
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanReview { get; set; }
        public UserDto? User { get; set; }
    }

    public class OrderItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class ShippingInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class OrderTrackingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }

    public class CreateOrderRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
        public ShippingInfoDto ShippingInfo { get; set; } = new ShippingInfoDto();
        public string PaymentMethod { get; set; } = "COD";
        public string? Note { get; set; }
    }

    public class CreateOrderItemRequest
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

    public class UpdateOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class UpdateOrderRequest
    {
        public string? Status { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Shipping { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Total { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentId { get; set; }
        public ShippingInfoDto? ShippingInfo { get; set; }
        public string? Note { get; set; }
    }
}

