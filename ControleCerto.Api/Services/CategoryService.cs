using AutoMapper;
using ControleCerto.DTOs.Category;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using MassTransit.Initializers;
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

            await _appDbContext.SaveChangesAsync();

            if (createdCategory is null)
            {
                return new AppError("Não foi possível criar a categoria, aguarde um momento e tente novamente, caso persistir, entre em contato.", ErrorTypeEnum.InternalError);
            }

            if (request.Limit is not null)
            {
                try
                {
                    await SetNewCategoryLimitAsync(createdCategory.Entity.Id, (double)request.Limit);
                }
                catch (Exception e)
                {
                    return new AppError($"Não foi possível registrar o limite: { e.Message }", ErrorTypeEnum.InternalError);
                }

            }

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
                    Limit = _appDbContext.CategoryLimits
                                .Where(cl => cl.CategoryId == c.Id && cl.EndDate == null)
                                .OrderByDescending(l => l.StartDate)
                                .Select(l => l.Amount)
                                .FirstOrDefault(),
                    SubCategories = categories
                        .Where(sc => sc.ParentId == c.Id)
                        .Select(sc => new InfoCategoryResponse
                        {
                            Id = sc.Id,
                            parentId = sc.ParentId,
                            Name = sc.Name,
                            Icon = sc.Icon,
                            BillType = sc.BillType,
                            Color = sc.Color,
                            Limit = _appDbContext.CategoryLimits
                                .Where(cl => cl.CategoryId == sc.Id && cl.EndDate == null)
                                .OrderByDescending(l => l.StartDate)
                                .Select(l => l.Amount)
                                .FirstOrDefault()
                        })
                .ToList()
                })
                .ToList();

            return parentCategories;
        }

        public async Task<Result<InfoCategoryResponse>> UpdateCategoryLimitAsync(UpdateCategoryLimitRequest data, int userId)
        {
            var category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == data.CategoryId && c.UserId == userId);

            if (category == null)
            {
                return new AppError("Categoria não encontrada.", ErrorTypeEnum.NotFound);
            }

            try
            {
                await SetNewCategoryLimitAsync(data.CategoryId, data.Amount);

                var infoCategory = _mapper.Map<InfoCategoryResponse>(category);
                infoCategory.Limit = data.Amount;

                return infoCategory;
            }
            catch (Exception ex)
            {
                return new AppError(ex.Message, ErrorTypeEnum.InternalError);
            }
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
            if (request.ParentId is not null)
                categoryToUpdate.ParentId = request.ParentId;

            var updatedCategory = _appDbContext.Categories.Update(categoryToUpdate);
            await _appDbContext.SaveChangesAsync();

            var InfoCategory = _mapper.Map<InfoCategoryResponse>(updatedCategory.Entity);
            InfoCategory.Limit = await _appDbContext.CategoryLimits.FirstOrDefaultAsync(
                cl => cl.CategoryId == request.Id 
                && cl.EndDate == null)
                .Select(cl => cl?.Amount ?? null);

            return InfoCategory;
        }

        private async Task SetNewCategoryLimitAsync(long categoryId, double amount)
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var lastLimit = await _appDbContext.CategoryLimits.FirstOrDefaultAsync(cl => cl.EndDate == null && cl.CategoryId == categoryId);


            if (lastLimit is not null)
            {
                if (lastLimit.StartDate == firstDayOfMonth)
                {
                    lastLimit.Amount = amount;
                    _appDbContext.Update(lastLimit);
                    await _appDbContext.SaveChangesAsync();
                    return;
                }
                else
                {
                    lastLimit.EndDate = firstDayOfMonth.AddSeconds(-1);
                    _appDbContext.Update(lastLimit);
                }

            }

            var newLimt = new CategoryLimit
            {
                Amount = amount,
                CategoryId = categoryId,
                StartDate = firstDayOfMonth,
                EndDate = null
            };

            await _appDbContext.CategoryLimits.AddAsync(newLimt);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<Result<InfoLimitResponse>> GetLimitInfo(long categoryId, int userId)
        {
            var now = DateTime.UtcNow;
            var actualMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var categoryIdSearch = categoryId;

            Category? category = null;
            var limits = new List<CategoryLimit>();

            for(var i = 0; i <= 1; i++)
            {
                category = await _appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryIdSearch && c.UserId == userId);

                if (category is null)
                {
                    return new AppError("Categória não encontrada.", ErrorTypeEnum.NotFound);
                }

                limits = await _appDbContext.CategoryLimits
                .Where(cl => cl.CategoryId == category.Id)
                .OrderBy(cl => cl.StartDate)
                .ToListAsync();

                bool hasActiveLimit = limits.Any(cl => cl.CategoryId == category.Id && cl.EndDate == null);

                if (limits.Count == 0 || !hasActiveLimit)
                {
                    if (category.ParentId.HasValue)
                    {
                        categoryIdSearch = category.ParentId.Value;
                    }
                    else
                    {
                        return new InfoLimitResponse
                        {
                            IsParentLimit = i != 0,
                            AccumulatedLimit = 0,
                            ActualLimit = 0,
                            AvailableMonthLimit = 0
                        };
                    }
                }
            }

            var firstLimit = limits.OrderBy(cl => cl.StartDate).FirstOrDefault()!;
            var currentLimit = limits.FirstOrDefault(cl => cl.EndDate == null)!;

            var transactions = await _appDbContext.Transactions.Where(t => t.CategoryId == category!.Id && t.PurchaseDate >= firstLimit.StartDate).ToListAsync();

            double accumulatedLimit = 0;

            foreach (var limit in limits)
            {
                var startDate = limit.StartDate;
                var endDate = limit.EndDate ?? now;

                var transactionsInPeriod = transactions
                    .Where(t => t.PurchaseDate >= startDate && t.PurchaseDate < endDate)
                    .Sum(t => t.Amount);

                // Calcular quantos mesês esse limite foi vigente
                int yearDiff = endDate.Year - startDate.Year;
                int monthDiff = endDate.Month - startDate.Month;

                var months = (yearDiff * 12) + monthDiff + 1;

                var positiveLimit = limit.Amount * months;

                accumulatedLimit += positiveLimit - transactionsInPeriod;
            }

            var infoLimit = new InfoLimitResponse
            {
                IsParentLimit = categoryIdSearch != categoryId,
                ActualLimit = currentLimit.Amount,
                AvailableMonthLimit = Math.Round(
                    currentLimit.Amount -
                        transactions.Where(t => t.PurchaseDate >= actualMonth).Sum(t => t.Amount),
                    2),
                 AccumulatedLimit = Math.Round(accumulatedLimit, 2)
            };

            return infoLimit;
        }
    }
}
