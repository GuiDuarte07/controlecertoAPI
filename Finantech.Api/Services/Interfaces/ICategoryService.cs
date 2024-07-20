using Finantech.DTOs.Category;
using Finantech.Enums;
using Finantech.Errors;
using Finantech.Models.Entities;

namespace Finantech.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<Result<InfoCategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, int userId);
        public Task<Result<InfoCategoryResponse>> UpdateCategoryAsync(UpdateCategoryRequest request, int userId);
        public Task<Result<bool>> DeleteCategoryAsync(int categoryId, int userId);
        public Task<Result<ICollection<InfoCategoryResponse>>> GetAllCategoriesAsync(int userId, BillTypeEnum? type);
        public Task SetAllDefaultCatogoriesAsync(int userId);
    }
}
