using be_lecas.DTOs;
using be_lecas.Common;

namespace be_lecas.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request);
        Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(string userId, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(string orderId, string userId);
        Task<ApiResponse<bool>> CancelOrderAsync(string orderId, string userId);
        Task<ApiResponse<ReviewDto>> CreateOrderReviewAsync(string orderId, string userId, CreateReviewRequest request);
        Task<ApiResponse<List<OrderTrackingDto>>> GetOrderTrackingAsync(string orderId, string userId);
        Task<ApiResponse<OrderDto?>> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequest request);
        Task<ApiResponse> UpdateOrderInfoAsync(string orderId, string userId, UpdateOrderRequest request);
    }
}

