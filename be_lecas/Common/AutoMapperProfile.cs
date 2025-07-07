using AutoMapper;
using be_lecas.Models;
using be_lecas.DTOs;

namespace be_lecas.Common
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User -> UserDto mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles ?? new List<string>()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // CreateUserRequest -> User mapping
            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerificationToken, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerificationExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => new List<string> { "user" }));

            // UpdateUserRequest -> User mapping
            CreateMap<UpdateUserRequest, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Product mappings
            CreateMap<Product, ProductDto>();
            CreateMap<CreateProductRequest, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.InStock, opt => opt.MapFrom(src => src.StockQuantity > 0))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 0.0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => 0));
            CreateMap<UpdateProductRequest, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ProductColor, ProductColorDto>();
            CreateMap<ProductColorDto, ProductColor>();
            
            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryRequest, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
            CreateMap<UpdateCategoryRequest, Category>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateOrderRequest, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Cart mappings
            CreateMap<Cart, CartDto>();
            CreateMap<CreateCartRequest, Cart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Items.Count))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Shipping, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => 0));
            CreateMap<UpdateCartRequest, Cart>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CartItem, CartItemDto>();
            CreateMap<AddToCartRequest, CartItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SelectedSize, opt => opt.MapFrom(src => src.Size))
                .ForMember(dest => dest.SelectedColor, opt => opt.MapFrom(src => src.Color));
            CreateMap<UpdateCartItemRequest, CartItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            // Review mappings
            CreateMap<Review, ReviewDto>();
        }
    }
} 