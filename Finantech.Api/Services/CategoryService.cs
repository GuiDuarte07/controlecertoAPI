using AutoMapper;
using Finantech.DTOs.Category;
using Finantech.Enums;
using Finantech.Errors;
using Finantech.Models.AppDbContext;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Finantech.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public CategoryService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<InfoCategoryResponse>> CreateCategoryAsync(CreateCategoryRequest request, int userId)
        {
            //FAZER O MAP
            var categoryToCreate = _mapper.Map<Category>(request);
            categoryToCreate.UserId = userId;

            var createdCategory = await _appDbContext.Categories.AddAsync(categoryToCreate);

            if (createdCategory is null)
            {
                return new AppError("Não foi possível criar a categoria, aguarde um momento e tente novamente, caso persistir, entre em contato.", ErrorTypeEnum.InternalError);
            }

            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCategoryResponse>(createdCategory.Entity);
        }

        public async Task<Result<bool>> DeleteCategoryAsync(int categoryId, int userId)
        {
            var categoryToDelete = await _appDbContext.Categories
                .Include(c => c.Transactions)
                .FirstAsync(c => c.Id == categoryId);

            if (categoryToDelete is null)
            {
                return new AppError("Categoria não encontrada.", ErrorTypeEnum.Validation);
            }

            if(!categoryToDelete.Transactions.Any())
            {
                _appDbContext.Categories.Remove(categoryToDelete);
                await _appDbContext.SaveChangesAsync();
            } else
            {
                return new AppError("Essa categoria possui registros e portanto não pode ser excluido. Tente desativa-lo.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        public async Task<Result<ICollection<InfoCategoryResponse>>> GetAllCategoriesAsync(int userId, BillTypeEnum? type)
        {
            var categories = await _appDbContext.Categories.Where(c => c.UserId == userId).ToListAsync();

            if (type.HasValue)
            {
                categories = categories.Where(c => c.BillType == type.Value).ToList();
            }

            var collection = _mapper.Map<List<InfoCategoryResponse>>(categories);

            return collection;
        }

        public async Task<Result<InfoCategoryResponse>> UpdateCategoryAsync(UpdateCategoryRequest request, int userId)
        {
            var categoryToUpdate = await _appDbContext.Categories.FirstAsync(e => e.Id == request.Id);
            
            if (categoryToUpdate is null || categoryToUpdate.UserId != userId)
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.Validation);
            }

            categoryToUpdate.UpdatedAt = DateTime.UtcNow;

            if (request.Icon is not null)
                categoryToUpdate.Icon = request.Icon;
            if (request.Name is not null)
                categoryToUpdate.Name = request.Name;
            if (request.Color is not null)
                categoryToUpdate.Color = request.Color;

            var updatedCategory = _appDbContext.Categories.Update(categoryToUpdate);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCategoryResponse>(updatedCategory.Entity);
        }
    }
}
