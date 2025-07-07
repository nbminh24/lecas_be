using be_lecas.DTOs;
using be_lecas.Common;

namespace be_lecas.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto?>> GetCategoryByIdAsync(string id);
        Task<ApiResponse<List<ProductDto>>> GetCategoryProductsAsync(string categoryId);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto?>> UpdateCategoryAsync(string id, UpdateCategoryRequest request);
        Task<ApiResponse<bool>> DeleteCategoryAsync(string id);
    }
} 