using AutoMapper;
using ControleCerto.DTOs.Category;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ControleCerto.Services
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

            if (categoryToCreate.ParentId is not null)
            {
                var parentCategory = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id  == categoryToCreate.ParentId);

                if (parentCategory is null)
                {
                    return new AppError("Categoria pai não encontrada.", ErrorTypeEnum.NotFound);
                }

                if (parentCategory.BillType != categoryToCreate.BillType)
                {
                    return new AppError("Tipo da categoria diferente da categoria pai.", ErrorTypeEnum.BusinessRule);
                }
            }

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
            .Where(c => c.Id == categoryId)
            .Select(c => new
            {
                Category = c,
                HasTransaction = c.Transactions.Count != 0
            })
            .FirstOrDefaultAsync();

            if (categoryToDelete is null || categoryToDelete.Category is null)
            {
                return new AppError("Categoria não encontrada.", ErrorTypeEnum.NotFound);
            }

            if(categoryToDelete.HasTransaction)
            {
                return new AppError("Essa categoria possui registros e portanto não pode ser excluído.", ErrorTypeEnum.BusinessRule);    
            }

            if (categoryToDelete.Category.ParentId is null)
            {
                var subcategories = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.ParentId == categoryToDelete.Category.Id);

                if (subcategories is not null) 
                {
                    return new AppError("Essa categoria possui sub-categorias e portanto não pode ser excluída.", ErrorTypeEnum.Validation);
                }
            }

            _appDbContext.Categories.Remove(categoryToDelete.Category);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Result<ICollection<InfoParentCategoryResponse>>> GetAllCategoriesAsync(int userId, BillTypeEnum? type)
        {
            var categories = await _appDbContext.Categories.Where(c => c.UserId == userId).ToListAsync();

            if (type.HasValue)
            {
                categories = categories.Where(c => c.BillType == type.Value).ToList();
            }

            var parentCategories = categories
                .Where(c => c.ParentId == null)
                .Select(c => new InfoParentCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    BillType = c.BillType,
                    Color = c.Color,
                    SubCategories = categories
                        .Where(sc => sc.ParentId == c.Id)
                        .Select(sc => new InfoCategoryResponse
                        {
                            Id = sc.Id,
                            parentId = sc.ParentId,
                            Name = sc.Name,
                            Icon = sc.Icon,
                            BillType = sc.BillType,
                            Color = sc.Color
                        })
                .ToList()
                })
                .ToList();

            return parentCategories;
        }

        public async Task<Result<InfoCategoryResponse>> UpdateCategoryAsync(UpdateCategoryRequest request, int userId)
        {
            var categoryToUpdate = await _appDbContext.Categories.FirstOrDefaultAsync(e => e.Id == request.Id);
            
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
