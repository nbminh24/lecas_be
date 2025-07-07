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
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<CartDto>.ErrorResult("Invalid token"));
                }

                var result = await _cartService.GetCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost("items")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<CartItemDto>.ErrorResult("Invalid token"));
                }

                var result = await _cartService.AddToCartAsync(userId, request);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetCart), result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartItemDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> UpdateCartItem(string itemId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<CartItemDto>.ErrorResult("Invalid token"));
                }

                var result = await _cartService.UpdateCartItemAsync(userId, itemId, request);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartItemDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromCart(string itemId)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _cartService.RemoveFromCartAsync(userId, itemId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> ClearCart()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _cartService.ClearCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCartSummary()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<CartSummaryDto>.ErrorResult("Invalid token"));
                }

                var result = await _cartService.GetCartSummaryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartSummaryDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }
}

