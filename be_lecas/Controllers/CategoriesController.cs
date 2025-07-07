using Microsoft.AspNetCore.Mvc;
using be_lecas.Common;
using be_lecas.DTOs;
using be_lecas.Services;
using Microsoft.AspNetCore.Authorization;

namespace be_lecas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
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

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto?>>> GetCategory(string id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetCategoryProducts(string id)
        {
            try
            {
                var result = await _categoryService.GetCategoryProductsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductDto>>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var result = await _categoryService.CreateCategoryAsync(request);
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetCategory), new { id = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto?>>> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, request);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto?>.ErrorResult($"Internal server error: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
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
    }
} 