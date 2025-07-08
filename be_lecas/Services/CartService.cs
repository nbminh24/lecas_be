using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;
using MongoDB.Bson;

namespace be_lecas.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CartDto>> GetCartAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    // Create new cart if doesn't exist
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    cart = await _cartRepository.CreateAsync(cart);
                }

                var cartDto = _mapper.Map<CartDto>(cart);

                // Load product information for each cart item
                foreach (var item in cartDto.Items)
                {
                    if (!string.IsNullOrEmpty(item.ProductId))
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            item.Product = _mapper.Map<ProductDto>(product);
                        }
                    }
                }

                return ApiResponse<CartDto>.SuccessResult(cartDto, "Cart retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CartDto>.ErrorResult($"Failed to get cart: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CartItemDto>> AddToCartAsync(string userId, AddToCartRequest request)
        {
            try
            {
                // Validate product exists
                var product = await _productRepository.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    return ApiResponse<CartItemDto>.ErrorResult("Product not found");
                }

                // Check if product is in stock
                if (!product.InStock || product.StockQuantity < request.Quantity)
                {
                    return ApiResponse<CartItemDto>.ErrorResult("Product is out of stock");
                }

                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    cart = await _cartRepository.CreateAsync(cart);
                }

                // Check if item already exists in cart
                var existingItem = cart.Items.FirstOrDefault(i => 
                    i.ProductId == request.ProductId && 
                    i.SelectedSize == request.Size && 
                    i.SelectedColor == request.Color);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        SelectedSize = request.Size,
                        SelectedColor = request.Color,
                        Price = product.Price,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    cart.Items.Add(newItem);
                }

                // Update cart totals
                UpdateCartTotals(cart);
                cart.UpdatedAt = DateTime.UtcNow;

                var updatedCart = await _cartRepository.UpdateAsync(cart);
                var cartItem = updatedCart.Items.Last(); // Get the newly added/updated item
                var cartItemDto = _mapper.Map<CartItemDto>(cartItem);
                cartItemDto.Product = _mapper.Map<ProductDto>(product);

                return ApiResponse<CartItemDto>.SuccessResult(cartItemDto, "Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CartItemDto>.ErrorResult($"Failed to add item to cart: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(string userId, string itemId, UpdateCartItemRequest request)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<CartItemDto>.ErrorResult("Cart not found");
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (cartItem == null)
                {
                    return ApiResponse<CartItemDto>.ErrorResult("Cart item not found");
                }

                // Update item properties
                if (request.Quantity.HasValue)
                {
                    if (request.Quantity.Value <= 0)
                    {
                        cart.Items.Remove(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity = request.Quantity.Value;
                    }
                }

                if (!string.IsNullOrEmpty(request.Size))
                {
                    cartItem.SelectedSize = request.Size;
                }

                if (!string.IsNullOrEmpty(request.Color))
                {
                    cartItem.SelectedColor = request.Color;
                }

                cartItem.UpdatedAt = DateTime.UtcNow;

                // Update cart totals
                UpdateCartTotals(cart);
                cart.UpdatedAt = DateTime.UtcNow;

                var updatedCart = await _cartRepository.UpdateAsync(cart);
                var updatedItem = updatedCart.Items.FirstOrDefault(i => i.Id == itemId);
                
                if (updatedItem == null)
                {
                    return ApiResponse<CartItemDto>.SuccessResult(default!, "Item removed from cart");
                }

                var cartItemDto = _mapper.Map<CartItemDto>(updatedItem);

                // Load product information
                var product = await _productRepository.GetByIdAsync(updatedItem.ProductId);
                if (product != null)
                {
                    cartItemDto.Product = _mapper.Map<ProductDto>(product);
                }

                return ApiResponse<CartItemDto>.SuccessResult(cartItemDto, "Cart item updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CartItemDto>.ErrorResult($"Failed to update cart item: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> RemoveFromCartAsync(string userId, string itemId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<bool>.ErrorResult("Cart not found");
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (cartItem == null)
                {
                    return ApiResponse<bool>.ErrorResult("Cart item not found");
                }

                cart.Items.Remove(cartItem);

                // Update cart totals
                UpdateCartTotals(cart);
                cart.UpdatedAt = DateTime.UtcNow;

                await _cartRepository.UpdateAsync(cart);

                return ApiResponse<bool>.SuccessResult(true, "Item removed from cart successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to remove item from cart: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Cart is already empty");
                }

                cart.Items.Clear();
                UpdateCartTotals(cart);
                cart.UpdatedAt = DateTime.UtcNow;

                await _cartRepository.UpdateAsync(cart);

                return ApiResponse<bool>.SuccessResult(true, "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to clear cart: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> GetCartSummaryAsync(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<CartSummaryDto>.SuccessResult(new CartSummaryDto(), "Cart is empty");
                }

                var summary = new CartSummaryDto
                {
                    TotalItems = cart.TotalItems,
                    Subtotal = cart.Subtotal,
                    Shipping = cart.Shipping,
                    Tax = cart.Tax,
                    Total = cart.Total
                };

                return ApiResponse<CartSummaryDto>.SuccessResult(summary, "Cart summary retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CartSummaryDto>.ErrorResult($"Failed to get cart summary: {ex.Message}");
            }
        }

        private void UpdateCartTotals(Cart cart)
        {
            cart.TotalItems = cart.Items.Sum(i => i.Quantity);
            cart.Subtotal = cart.Items.Sum(i => i.Price * i.Quantity);
            
            // Calculate shipping (free shipping for orders over 500k)
            cart.Shipping = cart.Subtotal >= 500000 ? 0 : 30000;
            
            // Calculate tax (0% for now)
            cart.Tax = 0;
            
            cart.Total = cart.Subtotal + cart.Shipping + cart.Tax;
        }
    }
}

