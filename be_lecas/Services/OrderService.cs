using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;

namespace be_lecas.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IEmailService emailService,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                // Validate cart items
                var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
                if (cart == null || !cart.Items.Any())
                {
                    return ApiResponse<OrderDto>.ErrorResult("Cart is empty");
                }

                // Create order items from cart
                var orderItems = new List<OrderItem>();
                decimal subtotal = 0;

                foreach (var cartItem in cart.Items)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        return ApiResponse<OrderDto>.ErrorResult($"Product {cartItem.ProductId} not found");
                    }

                    if (!product.InStock || product.StockQuantity < cartItem.Quantity)
                    {
                        return ApiResponse<OrderDto>.ErrorResult($"Product {product.Name} is out of stock");
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        Image = product.Images.FirstOrDefault(),
                        Quantity = cartItem.Quantity,
                        Size = cartItem.SelectedSize,
                        Color = cartItem.SelectedColor,
                        Price = cartItem.Price,
                        TotalPrice = cartItem.Price * cartItem.Quantity
                    };

                    orderItems.Add(orderItem);
                    subtotal += orderItem.TotalPrice;

                    // Update product stock
                    product.StockQuantity -= cartItem.Quantity;
                    product.InStock = product.StockQuantity > 0;
                    await _productRepository.UpdateAsync(product);
                }

                // Calculate totals
                var shipping = subtotal >= 500000 ? 0 : 30000; // Free shipping for orders over 500k
                var tax = 0; // 0% tax for now
                var total = subtotal + shipping + tax;

                // Create order
                var order = new Order
                {
                    UserId = request.UserId,
                    Status = OrderStatus.Pending,
                    Subtotal = subtotal,
                    Shipping = shipping,
                    Tax = tax,
                    Total = total,
                    PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod),
                    ShippingInfo = _mapper.Map<ShippingInfo>(request.ShippingInfo),
                    Items = orderItems,
                    Note = request.Note,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Tracking = new List<OrderTracking>
                    {
                        new OrderTracking
                        {
                            Status = "pending",
                            Location = "Hệ thống",
                            Description = "Đơn hàng đã được tạo",
                            Time = DateTime.UtcNow
                        }
                    }
                };

                var createdOrder = await _orderRepository.CreateAsync(order);

                // Clear cart after successful order creation
                cart.Items.Clear();
                await _cartRepository.UpdateAsync(cart);

                // Send confirmation email
                await _emailService.SendOrderConfirmationAsync(
                    request.ShippingInfo.Name, // TODO: Get user email
                    createdOrder.OrderNumber,
                    total
                );

                var orderDto = _mapper.Map<OrderDto>(createdOrder);
                return ApiResponse<OrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto>.ErrorResult($"Failed to create order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(string userId)
        {
            try
            {
                var orders = await _orderRepository.GetByUserIdAsync(userId);
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);

                return ApiResponse<List<OrderDto>>.SuccessResult(orderDtos, "Orders retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OrderDto>>.ErrorResult($"Failed to get orders: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrderDto?>> GetOrderByIdAsync(string orderId, string userId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto?>.ErrorResult("Order not found");
                }

                // Check if user owns this order
                if (order.UserId != userId)
                {
                    return ApiResponse<OrderDto?>.ErrorResult("Access denied");
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                orderDto.CanReview = order.Status == OrderStatus.Delivered;

                return ApiResponse<OrderDto?>.SuccessResult(orderDto, "Order retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto?>.ErrorResult($"Failed to get order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(string orderId, string userId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<bool>.ErrorResult("Order not found");
                }

                // Check if user owns this order
                if (order.UserId != userId)
                {
                    return ApiResponse<bool>.ErrorResult("Access denied");
                }

                // Check if order can be cancelled
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                {
                    return ApiResponse<bool>.ErrorResult("Order cannot be cancelled at this stage");
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                order.Tracking.Add(new OrderTracking
                {
                    Status = "cancelled",
                    Location = "Hệ thống",
                    Description = "Đơn hàng đã được hủy",
                    Time = DateTime.UtcNow
                });

                await _orderRepository.UpdateAsync(order);

                return ApiResponse<bool>.SuccessResult(true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to cancel order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReviewDto>> CreateOrderReviewAsync(string orderId, string userId, CreateReviewRequest request)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<ReviewDto>.ErrorResult("Order not found");
                }

                // Check if user owns this order
                if (order.UserId != userId)
                {
                    return ApiResponse<ReviewDto>.ErrorResult("Access denied");
                }

                // Check if order is delivered
                if (order.Status != OrderStatus.Delivered)
                {
                    return ApiResponse<ReviewDto>.ErrorResult("Order must be delivered before reviewing");
                }

                // TODO: Implement review creation
                var review = new Review
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    OrderId = orderId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    ImageUrl = request.ImageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                // TODO: Save review to database
                var reviewDto = _mapper.Map<ReviewDto>(review);

                return ApiResponse<ReviewDto>.SuccessResult(reviewDto, "Review created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ReviewDto>.ErrorResult($"Failed to create review: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OrderTrackingDto>>> GetOrderTrackingAsync(string orderId, string userId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<List<OrderTrackingDto>>.ErrorResult("Order not found");
                }

                // Check if user owns this order
                if (order.UserId != userId)
                {
                    return ApiResponse<List<OrderTrackingDto>>.ErrorResult("Access denied");
                }

                var trackingDtos = _mapper.Map<List<OrderTrackingDto>>(order.Tracking);

                return ApiResponse<List<OrderTrackingDto>>.SuccessResult(trackingDtos, "Tracking information retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<OrderTrackingDto>>.ErrorResult($"Failed to get tracking: {ex.Message}");
            }
        }

        public async Task<ApiResponse<OrderDto?>> UpdateOrderStatusAsync(string orderId, UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<OrderDto?>.ErrorResult("Order not found");
                }

                order.Status = Enum.Parse<OrderStatus>(request.Status);
                order.UpdatedAt = DateTime.UtcNow;

                // Add tracking entry
                order.Tracking.Add(new OrderTracking
                {
                    Status = request.Status.ToLower(),
                    Location = "Hệ thống",
                    Description = request.Note ?? $"Đơn hàng đã được cập nhật: {request.Status}",
                    Time = DateTime.UtcNow
                });

                var updatedOrder = await _orderRepository.UpdateAsync(order);
                var orderDto = _mapper.Map<OrderDto>(updatedOrder);

                return ApiResponse<OrderDto?>.SuccessResult(orderDto, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto?>.ErrorResult($"Failed to update order status: {ex.Message}");
            }
        }
    }
}

