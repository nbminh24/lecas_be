using be_lecas.DTOs;
using be_lecas.Common;

namespace be_lecas.Services
{
    public interface IProductService
    {
        Task<ApiResponse<List<ProductDto>>> GetAllProductsAsync(ProductFilterRequest? filter = null);
        Task<ApiResponse<ProductDto?>> GetProductByIdAsync(string id);
        Task<ApiResponse<List<ProductDto>>> GetProductsByCategoryAsync(string categoryId);
        Task<ApiResponse<List<ProductDto>>> SearchProductsAsync(string searchTerm);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);
        Task<ApiResponse<ProductDto?>> UpdateProductAsync(string id, UpdateProductRequest request);
        Task<ApiResponse<bool>> DeleteProductAsync(string id);
        Task<ApiResponse<List<ProductDto>>> GetRelatedProductsAsync(string productId);
        Task<ApiResponse<List<ProductDto>>> GetTopSellingProductsAsync();
        Task<ApiResponse<List<ProductDto>>> GetFlashSaleProductsAsync();
        Task<ApiResponse<int>> GetTotalProductsCountAsync(ProductFilterRequest? filter = null);
    }
}

