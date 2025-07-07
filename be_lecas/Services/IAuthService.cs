using be_lecas.Common;
using be_lecas.DTOs;

namespace be_lecas.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleAuthRequest request);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse> LogoutAsync(string userId, string refreshToken);
        Task<ApiResponse> VerifyEmailAsync(string token);
        Task<ApiResponse> ResendVerificationAsync(string email);
    }
}

