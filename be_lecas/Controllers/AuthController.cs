using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthResponse>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin([FromBody] GoogleAuthRequest request)
        {
            try
            {
                var result = await _authService.GoogleLoginAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthResponse>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthResponse>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _authService.LogoutAsync(userId, request.RefreshToken);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse>> VerifyEmail([FromBody] EmailVerificationRequest request)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(request.Token);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<ApiResponse>> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            try
            {
                var result = await _authService.ResendVerificationAsync(request.Email);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserDto>.ErrorResult("Invalid token"));
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
                }

                return Ok(ApiResponse<UserDto>.SuccessResult(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }
}

