using AutoMapper;
using Finantech.DTOs.Category;
using Finantech.DTOs.Expense;
using Finantech.Enums;
using Finantech.Models.AppDbContext;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public async Task<InfoCategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, int userId)
        {
            //FAZER O MAP
            var categoryToCreate = _mapper.Map<Category>(request);
            categoryToCreate.UserId = userId;

            var createdCategory = await _appDbContext.Categories.AddAsync(categoryToCreate);

            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoCategoryResponse>(createdCategory.Entity);
        }

        public async Task DeleteCategoryAsync(int categoryId, int userId)
        {
            var categoryToDelete = await _appDbContext.Categories
                .Include(c => c.Expenses)
                .Include(c => c.Incomes)
                .Include(c => c.CreditExpenses)
                .FirstOrDefaultAsync(c => c.Id == categoryId) 
                ?? throw new Exception("Categoria não encontrada");

            if(!categoryToDelete.Incomes.Any() && !categoryToDelete.Expenses.Any() && !categoryToDelete.CreditExpenses.Any())
            {
                _appDbContext.Categories.Remove(categoryToDelete);
                await _appDbContext.SaveChangesAsync();
            } else
            {
                throw new Exception("Essa categoria possui registros e portanto não pode ser excluido. Tente desativa-lo.");
            }

            return;
        }

        public async Task<ICollection<InfoCategoryResponse>> GetAllCategoriesAsync(int userId)
        {
            var catogories = await _appDbContext.Categories.Where(c => c.UserId == userId).ToListAsync();

            return _mapper.Map<ICollection<InfoCategoryResponse>>(catogories);
        }

        public async Task<InfoCategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request, int userId)
        {
            var categoryToUpdate = await _appDbContext.Categories.FirstOrDefaultAsync(e => e.Id == request.Id) 
                ?? throw new Exception("Conta não encontrada.");

            categoryToUpdate.UpdatedAt = DateTime.Now;

            if (categoryToUpdate.UserId != userId)
            {
                throw new Exception("Não autorizado: Categoria não pertence a usuário.");
            }

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

        public async Task SetAllDefaultCatogoriesAsync(int userId)
        {
            try
            {
                var categoriesFromDefaults = await _appDbContext.CategoriesDefault.Select(df => new Category(df, userId)).ToListAsync();

                await _appDbContext.Categories.AddRangeAsync(categoriesFromDefaults);
                await _appDbContext.SaveChangesAsync();

            } catch (Exception ex)
            {
                throw;
            }
        }

    }
}
