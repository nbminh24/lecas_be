using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IProductService _productService;

        public SearchController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> SearchProducts([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return BadRequest(ApiResponse<List<ProductDto>>.ErrorResult("Keyword is required"));
                }

                var result = await _productService.SearchProductsAsync(keyword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("suggestions")]
        public ActionResult<ApiResponse<List<SearchSuggestionDto>>> GetSuggestions([FromQuery] string keyword)
        {
            try
            {
                // TODO: Implement search suggestions
                var suggestions = new List<SearchSuggestionDto>
                {
                    new SearchSuggestionDto { Keyword = "áo thun", Count = 10 },
                    new SearchSuggestionDto { Keyword = "quần jean", Count = 5 },
                    new SearchSuggestionDto { Keyword = "áo sơ mi", Count = 8 }
                };

                return Ok(ApiResponse<List<SearchSuggestionDto>>.SuccessResult(suggestions, "Suggestions retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<SearchSuggestionDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }
    }

    public class SearchSuggestionDto
    {
        public string Keyword { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

