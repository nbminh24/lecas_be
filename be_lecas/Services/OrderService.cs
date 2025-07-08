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
        private readonly IPromotionRepository _promotionRepository;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IEmailService emailService,
            IMapper mapper,
            IPromotionRepository promotionRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _emailService = emailService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
        {
            try
            {
                if (request.Items == null || !request.Items.Any())
                {
                    return ApiResponse<OrderDto>.ErrorResult("No items selected for order");
                }

                // Lấy danh sách promotion đang hoạt động
                var activePromotions = await _promotionRepository.GetActivePromotionsAsync();

                // Lấy giỏ hàng hiện tại của user (nếu có)
                var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
                var orderItems = new List<OrderItem>();
                decimal subtotal = 0;
                var cartItemsToRemove = new List<CartItem>();

                // Bước 1: Kiểm tra tồn kho đồng bộ cho tất cả sản phẩm trước khi trừ kho
                foreach (var reqItem in request.Items)
                {
                    var product = await _productRepository.GetByIdAsync(reqItem.ProductId);
                    if (product == null)
                    {
                        return ApiResponse<OrderDto>.ErrorResult($"Product {reqItem.ProductId} not found");
                    }
                    if (!product.InStock || product.StockQuantity < reqItem.Quantity)
                    {
                        return ApiResponse<OrderDto>.ErrorResult($"Product {product.Name} is out of stock");
                    }
                }

                // Nếu tất cả sản phẩm đều đủ tồn kho, tiến hành trừ kho và tạo order
                foreach (var reqItem in request.Items)
                {
                    var product = await _productRepository.GetByIdAsync(reqItem.ProductId);
                    if (product == null)
                    {
                        return ApiResponse<OrderDto>.ErrorResult($"Product {reqItem.ProductId} not found");
                    }
                    decimal price = product.Price;
                    // Áp dụng promotion nếu có
                    var promo = activePromotions.FirstOrDefault(p => p.ProductIds.Contains(product.Id));
                    decimal discount = 0;
                    if (promo != null)
                    {
                        if (promo.DiscountType == "percent")
                        {
                            discount = price * promo.DiscountValue / 100m;
                        }
                        else if (promo.DiscountType == "amount")
                        {
                            discount = promo.DiscountValue;
                        }
                        if (discount > price) discount = price;
                        price -= discount;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = reqItem.ProductId,
                        ProductName = product.Name,
                        Image = product.Images.FirstOrDefault(),
                        Quantity = reqItem.Quantity,
                        Size = reqItem.Size,
                        Color = reqItem.Color,
                        Price = price,
                        TotalPrice = price * reqItem.Quantity
                    };
                    orderItems.Add(orderItem);
                    subtotal += orderItem.TotalPrice;

                    // Trừ tồn kho (đảm bảo đồng bộ)
                    product.StockQuantity -= reqItem.Quantity;
                    product.InStock = product.StockQuantity > 0;
                    await _productRepository.UpdateAsync(product);

                    // Nếu sản phẩm này có trong giỏ thì đánh dấu để xóa
                    if (cart != null)
                    {
                        var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == reqItem.ProductId && x.SelectedSize == reqItem.Size && x.SelectedColor == reqItem.Color);
                        if (cartItem != null)
                        {
                            cartItemsToRemove.Add(cartItem);
                        }
                    }
                }

                // Tính phí ship, thuế, tổng tiền
                var shipping = subtotal >= 500000 ? 0 : 30000;
                var tax = 0;
                var total = subtotal + shipping + tax;

                // Validate dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(request.ShippingInfo?.Name) ||
                    string.IsNullOrWhiteSpace(request.ShippingInfo?.Phone) ||
                    string.IsNullOrWhiteSpace(request.ShippingInfo?.Address))
                {
                    return ApiResponse<OrderDto>.ErrorResult("Thông tin giao nhận không hợp lệ");
                }
                if (string.IsNullOrWhiteSpace(request.PaymentMethod) ||
                    !(request.PaymentMethod == "COD" || request.PaymentMethod == "MoMo" || request.PaymentMethod == "VNPay"))
                {
                    return ApiResponse<OrderDto>.ErrorResult("Phương thức thanh toán không hợp lệ");
                }

                // Parse payment method an toàn
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var paymentMethod))
                {
                    return ApiResponse<OrderDto>.ErrorResult("Phương thức thanh toán không hợp lệ (enum)");
                }

                // Tạo đơn hàng
                var order = new Order
                {
                    UserId = request.UserId,
                    Status = OrderStatus.Pending,
                    Subtotal = subtotal,
                    Shipping = shipping,
                    Tax = tax,
                    Total = total,
                    PaymentMethod = paymentMethod,
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
                    },
                    History = new List<OrderHistory>
                    {
                        new OrderHistory
                        {
                            Status = "pending",
                            ChangedBy = request.UserId,
                            Note = "Tạo đơn hàng",
                            ChangedAt = DateTime.UtcNow
                        }
                    }
                };

                var createdOrder = await _orderRepository.CreateAsync(order);

                // Xóa các sản phẩm đã đặt khỏi giỏ hàng
                if (cart != null && cartItemsToRemove.Any())
                {
                    foreach (var item in cartItemsToRemove)
                    {
                        cart.Items.Remove(item);
                    }
                    await _cartRepository.UpdateAsync(cart);
                }

                // Gửi email xác nhận đơn hàng
                await _emailService.SendOrderConfirmationAsync(
                    request.ShippingInfo.Name, // TODO: Get user email
                    createdOrder.OrderNumber,
                    total
                );
                // Gửi email thông báo trạng thái đơn hàng (tạo mới)
                await _emailService.SendOrderStatusUpdateAsync(
                    request.ShippingInfo?.Name ?? "Customer", // TODO: Get user email
                    createdOrder.OrderNumber,
                    "pending",
                    "Đơn hàng của bạn đã được tạo thành công."
                );

                var orderDto = _mapper.Map<OrderDto>(createdOrder);
                return ApiResponse<OrderDto>.SuccessResult(orderDto, "Order created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto>.ErrorResult($"Failed to create order: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(string userId, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var orders = await _orderRepository.GetByUserIdAsync(userId);
                if (!string.IsNullOrEmpty(status))
                {
                    orders = orders.Where(o => o.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                if (fromDate.HasValue)
                {
                    orders = orders.Where(o => o.CreatedAt >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    orders = orders.Where(o => o.CreatedAt <= toDate.Value).ToList();
                }
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
                // Set canReview cho từng sản phẩm
                if (order.Status == OrderStatus.Delivered)
                {
                    foreach (var item in orderDto.Items)
                    {
                        // Giả sử có hàm kiểm tra đã review chưa (cần bổ sung logic thực tế)
                        item.CanReview = !await HasUserReviewedProductInOrder(userId, item.ProductId, order.Id);
                    }
                }

                return ApiResponse<OrderDto?>.SuccessResult(orderDto, "Order retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto?>.ErrorResult($"Failed to get order: {ex.Message}");
            }
        }

        // Hàm kiểm tra user đã review sản phẩm này trong đơn này chưa
        private Task<bool> HasUserReviewedProductInOrder(string userId, string productId, string orderId)
        {
            // TODO: Thực hiện truy vấn thực tế tới review repository
            // Giả lập: luôn trả về false (chưa review)
            return Task.FromResult(false);
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(string orderId, string userId, string? reason = null)
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
                order.History.Add(new OrderHistory
                {
                    Status = "cancelled",
                    ChangedBy = userId,
                    Note = reason ?? "User cancelled order",
                    ChangedAt = DateTime.UtcNow
                });
                order.CancelReason = reason ?? "User cancelled order";

                await _orderRepository.UpdateAsync(order);

                // Gửi email thông báo trạng thái đơn hàng (hủy)
                await _emailService.SendOrderStatusUpdateAsync(
                    order.ShippingInfo?.Name ?? "Customer", // TODO: Get user email
                    order.OrderNumber,
                    "cancelled",
                    "Đơn hàng của bạn đã bị hủy."
                );

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

                // Kiểm tra user đã review sản phẩm này trong đơn này chưa
                if (await HasUserReviewedProductInOrder(userId, request.ProductId, orderId))
                {
                    return ApiResponse<ReviewDto>.ErrorResult("You have already reviewed this product in this order");
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

                // Gửi email thông báo trạng thái đơn hàng (cập nhật)
                await _emailService.SendOrderStatusUpdateAsync(
                    order.ShippingInfo?.Name ?? "Customer", // TODO: Get user email
                    order.OrderNumber,
                    request.Status.ToLower(),
                    $"Đơn hàng của bạn đã được cập nhật: {request.Status}"
                );

                return ApiResponse<OrderDto?>.SuccessResult(orderDto, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<OrderDto?>.ErrorResult($"Failed to update order status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> UpdateOrderInfoAsync(string orderId, string userId, UpdateOrderRequest request)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return ApiResponse<object>.ErrorResult("Order not found");
                }
                if (order.UserId != userId)
                {
                    return ApiResponse<object>.ErrorResult("Access denied");
                }
                if (order.Status != OrderStatus.Pending)
                {
                    return ApiResponse<object>.ErrorResult("Order cannot be updated at this stage");
                }
                // Cập nhật các trường cho phép
                if (request.ShippingInfo != null)
                {
                    order.ShippingInfo = _mapper.Map<ShippingInfo>(request.ShippingInfo);
                }
                if (!string.IsNullOrWhiteSpace(request.Note))
                {
                    order.Note = request.Note;
                }
                order.UpdatedAt = DateTime.UtcNow;
                order.History.Add(new OrderHistory
                {
                    Status = order.Status.ToString(),
                    ChangedBy = userId,
                    Note = "User updated order info",
                    ChangedAt = DateTime.UtcNow
                });
                await _orderRepository.UpdateAsync(order);
                return ApiResponse<object>.SuccessResult(new { }, "Order info updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult($"Failed to update order info: {ex.Message}");
            }
        }
    }
}

