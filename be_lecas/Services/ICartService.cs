using be_lecas.DTOs;
using be_lecas.Common;

namespace be_lecas.Services
{
    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartAsync(string userId);
        Task<ApiResponse<CartItemDto>> AddToCartAsync(string userId, AddToCartRequest request);
        Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(string userId, string itemId, UpdateCartItemRequest request);
        Task<ApiResponse<bool>> RemoveFromCartAsync(string userId, string itemId);
        Task<ApiResponse<bool>> ClearCartAsync(string userId);
        Task<ApiResponse<CartSummaryDto>> GetCartSummaryAsync(string userId);
    }
}

