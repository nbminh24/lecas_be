using be_lecas.DTOs;
using be_lecas.Common;

namespace be_lecas.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(string userId);
        Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task<ApiResponse<UserDto?>> AddAddressAsync(string userId, AddressRequest request);
        Task<ApiResponse> RemoveAddressAsync(string userId, string addressId);
    }
}

