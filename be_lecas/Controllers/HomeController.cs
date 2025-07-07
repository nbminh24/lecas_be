using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public HomeController(IProductService productService, ICategoryService categoryService, ICacheService cacheService, IConfiguration configuration)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cacheService = cacheService;
            _configuration = configuration;
        }

        [HttpGet("featured-products")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetFeaturedProducts()
        {
            try
            {
                var cacheKey = "featured_products";
                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                
                if (cached != null)
                {
                    return Ok(ApiResponse<List<ProductDto>>.SuccessResult(cached, "Featured products retrieved from cache"));
                }

                var result = await _productService.GetTopSellingProductsAsync();
                
                if (result.Success && result.Data != null)
                {
                    var expiry = TimeSpan.Parse(_configuration["CacheSettings:FeaturedProductsExpiry"] ?? "00:30:00");
                    await _cacheService.SetAsync(cacheKey, result.Data, expiry);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("flash-sale")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetFlashSale()
        {
            try
            {
                var cacheKey = "flash_sale_products";
                var cached = await _cacheService.GetAsync<List<ProductDto>>(cacheKey);
                
                if (cached != null)
                {
                    return Ok(ApiResponse<List<ProductDto>>.SuccessResult(cached, "Flash sale products retrieved from cache"));
                }

                var result = await _productService.GetFlashSaleProductsAsync();
                
                if (result.Success && result.Data != null)
                {
                    var expiry = TimeSpan.Parse(_configuration["CacheSettings:FlashSaleExpiry"] ?? "00:15:00");
                    await _cacheService.SetAsync(cacheKey, result.Data, expiry);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var cacheKey = "categories";
                var cached = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey);
                
                if (cached != null)
                {
                    return Ok(ApiResponse<List<CategoryDto>>.SuccessResult(cached, "Categories retrieved from cache"));
                }

                var result = await _categoryService.GetAllCategoriesAsync();
                
                if (result.Success && result.Data != null)
                {
                    var expiry = TimeSpan.Parse(_configuration["CacheSettings:CategoriesExpiry"] ?? "01:00:00");
                    await _cacheService.SetAsync(cacheKey, result.Data, expiry);
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("stats")]
        public ActionResult<ApiResponse<HomeStatsDto>> GetStats()
        {
            try
            {
                // TODO: Implement actual stats
                var stats = new HomeStatsDto
                {
                    TotalProducts = 1000,
                    TotalCategories = 50,
                    TotalOrders = 5000,
                    TotalUsers = 2000
                };

                return Ok(ApiResponse<HomeStatsDto>.SuccessResult(stats, "Stats retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<HomeStatsDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }

    public class HomeStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
    }
} 