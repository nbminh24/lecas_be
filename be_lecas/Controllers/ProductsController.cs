using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;
using Microsoft.AspNetCore.Authorization;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts([FromQuery] ProductFilterRequest filter)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto?>>> GetProduct(string id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}/reviews")]
        public ActionResult<ApiResponse<List<ReviewDto>>> GetProductReviews(string id)
        {
            try
            {
                // TODO: Implement review service
                return Ok(ApiResponse<List<ReviewDto>>.SuccessResult(new List<ReviewDto>(), "Reviews retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ReviewDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}/related")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetRelatedProducts(string id)
        {
            try
            {
                var result = await _productService.GetRelatedProductsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var result = await _productService.CreateProductAsync(request);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetProduct), new { id = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<ProductDto?>>> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, request);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(string id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("top-selling")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetTopSellingProducts()
        {
            try
            {
                var result = await _productService.GetTopSellingProductsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("flash-sale")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetFlashSaleProducts()
        {
            try
            {
                var result = await _productService.GetFlashSaleProductsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }
}

