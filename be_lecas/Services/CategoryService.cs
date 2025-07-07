using be_lecas.DTOs;
using be_lecas.Models;
using be_lecas.Repositories;
using be_lecas.Common;
using AutoMapper;

namespace be_lecas.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

                // Build hierarchy
                var rootCategories = categoryDtos.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();
                foreach (var rootCategory in rootCategories)
                {
                    BuildCategoryHierarchy(rootCategory, categoryDtos);
                }

                return ApiResponse<List<CategoryDto>>.SuccessResult(rootCategories, "Categories retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<CategoryDto>>.ErrorResult($"Failed to get categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto?>> GetCategoryByIdAsync(string id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<CategoryDto?>.ErrorResult("Category not found");
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);

                // Load parent category if exists
                if (!string.IsNullOrEmpty(category.ParentId))
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(category.ParentId);
                    if (parentCategory != null)
                    {
                        categoryDto.Parent = _mapper.Map<CategoryDto>(parentCategory);
                    }
                }

                // Load children categories
                var children = await _categoryRepository.GetChildrenAsync(id);
                categoryDto.Children = _mapper.Map<List<CategoryDto>>(children);

                return ApiResponse<CategoryDto?>.SuccessResult(categoryDto, "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto?>.ErrorResult($"Failed to get category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductDto>>> GetCategoryProductsAsync(string categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(categoryId);
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                // Load category information for each product
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category != null)
                {
                    var categoryDto = _mapper.Map<CategoryDto>(category);
                    foreach (var productDto in productDtos)
                    {
                        productDto.Category = categoryDto;
                    }
                }

                return ApiResponse<List<ProductDto>>.SuccessResult(productDtos, "Category products retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProductDto>>.ErrorResult($"Failed to get category products: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                // Validate parent category if provided
                if (!string.IsNullOrEmpty(request.ParentId))
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentId);
                    if (parentCategory == null)
                    {
                        return ApiResponse<CategoryDto>.ErrorResult("Parent category not found");
                    }
                }

                var category = _mapper.Map<Category>(request);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                category.IsActive = true;

                var createdCategory = await _categoryRepository.CreateAsync(category);
                var categoryDto = _mapper.Map<CategoryDto>(createdCategory);

                return ApiResponse<CategoryDto>.SuccessResult(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto>.ErrorResult($"Failed to create category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CategoryDto?>> UpdateCategoryAsync(string id, UpdateCategoryRequest request)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryDto?>.ErrorResult("Category not found");
                }

                // Validate parent category if provided
                if (!string.IsNullOrEmpty(request.ParentId))
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentId);
                    if (parentCategory == null)
                    {
                        return ApiResponse<CategoryDto?>.ErrorResult("Parent category not found");
                    }

                    // Prevent circular reference
                    if (request.ParentId == id)
                    {
                        return ApiResponse<CategoryDto?>.ErrorResult("Category cannot be its own parent");
                    }
                }

                // Update properties
                if (!string.IsNullOrEmpty(request.Name))
                    existingCategory.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    existingCategory.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Image))
                    existingCategory.Image = request.Image;

                if (!string.IsNullOrEmpty(request.ParentId))
                    existingCategory.ParentId = request.ParentId;

                if (request.SortOrder.HasValue)
                    existingCategory.SortOrder = request.SortOrder.Value;

                if (request.IsActive.HasValue)
                    existingCategory.IsActive = request.IsActive.Value;

                existingCategory.UpdatedAt = DateTime.UtcNow;

                var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
                var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);

                return ApiResponse<CategoryDto?>.SuccessResult(categoryDto, "Category updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CategoryDto?>.ErrorResult($"Failed to update category: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(string id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResult("Category not found");
                }

                // Check if category has children
                var children = await _categoryRepository.GetChildrenAsync(id);
                if (children.Any())
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete category with children");
                }

                // Check if category has products
                var products = await _productRepository.GetByCategoryAsync(id);
                if (products.Any())
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete category with products");
                }

                var result = await _categoryRepository.DeleteAsync(id);
                if (result)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Category deleted successfully");
                }
                return ApiResponse<bool>.ErrorResult("Failed to delete category");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete category: {ex.Message}");
            }
        }

        private void BuildCategoryHierarchy(CategoryDto parent, List<CategoryDto> allCategories)
        {
            parent.Children = allCategories.Where(c => c.ParentId == parent.Id).ToList();
            foreach (var child in parent.Children)
            {
                BuildCategoryHierarchy(child, allCategories);
            }
        }
    }
} 