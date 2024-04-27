using Finantech.DTOs.Category;
using Finantech.Models.Entities;

namespace Finantech.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<InfoCategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, int userId);
        public Task<InfoCategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request, int userId);
        public Task DeleteCategoryAsync(int categoryId, int userId);
        public Task<ICollection<InfoCategoryResponse>> GetAllCategoriesAsync(int userId);
    }
}
