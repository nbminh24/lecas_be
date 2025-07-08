using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using be_lecas.Models;
using be_lecas.Repositories;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public UsersController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto?>>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserDto?>.ErrorResult("Invalid token"));
                }

                var result = await _userService.GetUserByIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDto?>>> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserDto?>.ErrorResult("Invalid token"));
                }

                var result = await _userService.UpdateUserAsync(userId, request);
                if (result == null)
                {
                    return BadRequest(ApiResponse<UserDto?>.ErrorResult("Failed to update profile"));
                }
                return Ok(ApiResponse<UserDto?>.SuccessResult(result, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _userService.ChangePasswordAsync(userId, request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("addresses")]
        public async Task<ActionResult<ApiResponse<UserDto?>>> AddAddress([FromBody] AddressRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<UserDto?>.ErrorResult("Invalid token"));
                }

                var result = await _userService.AddAddressAsync(userId, request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("addresses/{addressId}")]
        public async Task<ActionResult<ApiResponse>> RemoveAddress(string addressId)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _userService.RemoveAddressAsync(userId, addressId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("addresses")]
        public async Task<ActionResult<ApiResponse<List<Address>>>> GetAddresses()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<Address>>.ErrorResult("Invalid token"));
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<List<Address>>.ErrorResult("User not found"));
            }

            // Lấy user model để trả về danh sách địa chỉ
            var userModel = await _userRepository.GetByIdAsync(userId);
            var addresses = userModel?.Addresses ?? new List<Address>();
            return Ok(ApiResponse<List<Address>>.SuccessResult(addresses, "Addresses retrieved successfully"));
        }
    }
}

