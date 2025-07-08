using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using Google.Apis.Auth;

namespace be_lecas.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly JwtHelper _jwtHelper;

        public AuthService(IUserRepository userRepository, IEmailService emailService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
            _jwtHelper = new JwtHelper(
                _configuration["JWT:SecretKey"]!,
                _configuration["JWT:Issuer"]!,
                _configuration["JWT:Audience"]!
            );
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Email không tồn tại");
                }

                if (!user.CanLoginWithPassword)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Tài khoản này chỉ có thể đăng nhập bằng Google");
                }

                if (!VerifyPassword(request.Password, user.PasswordHash!))
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Mật khẩu không đúng");
                }

                if (!user.IsEmailVerified)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Vui lòng xác nhận email trước khi đăng nhập");
                }

                return await GenerateAuthResponseAsync(user, false);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResult($"Đăng nhập thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleAuthRequest request)
        {
            try
            {
                // Validate Google access token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
                
                var user = await _userRepository.GetByEmailAsync(payload.Email);
                bool isNewUser = false;

                if (user == null)
                {
                    // Create new user from Google info
                    user = new User
                    {
                        Email = payload.Email,
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        GoogleId = payload.Subject,
                        Avatar = payload.Picture,
                        IsEmailVerified = true, // Google accounts are pre-verified
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _userRepository.CreateAsync(user);
                    isNewUser = true;

                    // Send welcome email
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
                }
                else
                {
                    // Update existing user's Google info
                    user.GoogleId = payload.Subject;
                    user.Avatar = payload.Picture;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);
                }

                return await GenerateAuthResponseAsync(user, isNewUser);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResult($"Đăng nhập Google thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
                if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                return await GenerateAuthResponseAsync(user, false);
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResult($"Làm mới token thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse> LogoutAsync(string userId, string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("Không tìm thấy người dùng");
                }

                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                return ApiResponse.SuccessResult("Đăng xuất thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResult($"Đăng xuất thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse> VerifyEmailAsync(string token)
        {
            try
            {
                var user = await _userRepository.GetByEmailVerificationTokenAsync(token);
                if (user == null || user.EmailVerificationExpiry <= DateTime.UtcNow)
                {
                    return ApiResponse.ErrorResult("Token xác nhận email không hợp lệ hoặc đã hết hạn");
                }

                user.IsEmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                return ApiResponse.SuccessResult("Xác nhận email thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResult($"Xác nhận email thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ResendVerificationAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("Email không tồn tại");
                }

                if (user.IsEmailVerified)
                {
                    return ApiResponse.ErrorResult("Email đã được xác nhận");
                }

                // Generate new verification token
                var token = GenerateVerificationToken();
                user.EmailVerificationToken = token;
                user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(user.Email, user.FullName, token);

                return ApiResponse.SuccessResult("Email xác nhận đã được gửi lại");
            }
            catch (Exception ex)
            {
                return ApiResponse.ErrorResult($"Gửi email xác nhận thất bại: {ex.Message}");
            }
        }

        private async Task<ApiResponse<AuthResponse>> GenerateAuthResponseAsync(User user, bool isNewUser)
        {
            // Generate tokens
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Update user with refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsNewUser = isNewUser,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Avatar = user.Avatar,
                    Roles = user.Roles,
                    IsEmailVerified = user.IsEmailVerified,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };

            return ApiResponse<AuthResponse>.SuccessResult(response, isNewUser ? "Đăng ký thành công" : "Đăng nhập thành công");
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private string GenerateVerificationToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}

