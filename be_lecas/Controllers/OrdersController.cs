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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OrderDto>.ErrorResult("Invalid token"));
                }

                request.UserId = userId; // Ensure the user can only create orders for themselves
                var result = await _orderService.CreateOrderAsync(request);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetOrder), new { id = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrders([FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<OrderDto>>.ErrorResult("Invalid token"));
                }

                var result = await _orderService.GetUserOrdersAsync(userId, status, fromDate, toDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto?>>> GetOrder(string id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<OrderDto?>.ErrorResult("Invalid token"));
                }

                var result = await _orderService.GetOrderByIdAsync(id, userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        public class CancelOrderRequest
        {
            public string? Reason { get; set; }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse>> CancelOrder(string id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _orderService.CancelOrderAsync(id, userId, request?.Reason);
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

        [HttpPost("{id}/reviews")]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateOrderReview(string id, [FromBody] CreateReviewRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<ReviewDto>.ErrorResult("Invalid token"));
                }

                var result = await _orderService.CreateOrderReviewAsync(id, userId, request);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetOrder), new { id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReviewDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}/tracking")]
        public async Task<ActionResult<ApiResponse<List<OrderTrackingDto>>>> GetOrderTracking(string id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<OrderTrackingDto>>.ErrorResult("Invalid token"));
                }

                var result = await _orderService.GetOrderTrackingAsync(id, userId);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<OrderTrackingDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<OrderDto?>>> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, request);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}/update-info")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderInfo(string id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResult("Invalid token"));
                }

                var result = await _orderService.UpdateOrderInfoAsync(id, userId, request);
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
    }
}

