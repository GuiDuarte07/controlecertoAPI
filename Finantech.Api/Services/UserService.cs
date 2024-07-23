using AutoMapper;
using Finantech.DTOs.User;
using Finantech.Enums;
using Finantech.Errors;
using Finantech.Models.AppDbContext;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finantech.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IHashService _hashService;
        private readonly ICategoryService _categoryService;

        public UserService(AppDbContext appDbContext, IMapper mapper, IHashService hashService, ICategoryService categoryService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _hashService = hashService;
            _categoryService = categoryService;
        }

        public async Task<Result<InfoUserResponse>> CreateUserAync(CreateUserRequest userReq)
        {
            var alreadyExistEmail = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == userReq.Email);

            if (alreadyExistEmail is not null) 
            {
                return new AppError("Já existe uma conta com esse e-mail cadastrada!", ErrorTypeEnum.Validation);
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {

                    var passHash = _hashService.HashPassword(userReq.Password);

                    var user = new User { Email = userReq.Email, Name = userReq.Name, PasswordHash = passHash };

                    var createdUser = _appDbContext.Users.Add(user);

                    await _appDbContext.SaveChangesAsync();

                    int userId = createdUser.Entity.Id;

                    var initialCategories = new List<Category>([
                        new Category("Casa", "home", "#00bfff", BillTypeEnum.EXPENSE, userId),
                        new Category("Educação", "import_contacts", "#ba55d3", BillTypeEnum.EXPENSE, userId),
                        new Category("Eletrônicos", "laptop_chromebook", "#ffef00", BillTypeEnum.EXPENSE, userId),
                        new Category("Lazer", "beach_access", "#ff9f00", BillTypeEnum.EXPENSE, userId),
                        new Category("Outros", "more_horiz", "#808080", BillTypeEnum.EXPENSE, userId),
                        new Category("Presentes", "featured_seasonal_and_gifts", "#9dc209", BillTypeEnum.EXPENSE, userId),
                        new Category("Restaurante", "restaurant", "#ae0c00", BillTypeEnum.EXPENSE, userId),
                        new Category("Saúde", "syringe", "#98fb98", BillTypeEnum.EXPENSE, userId),
                        new Category("Serviços", "work", "#228b22", BillTypeEnum.EXPENSE, userId),
                        new Category("Supermercado", "shopping_cart", "#e32636", BillTypeEnum.EXPENSE, userId),
                        new Category("Transporte", "local_taxi", "#00ced1", BillTypeEnum.EXPENSE, userId),
                        new Category("Vestuário", "checkroom", "#008b8b", BillTypeEnum.EXPENSE, userId),
                        new Category("Viagem", "travel", "#da70d6", BillTypeEnum.EXPENSE, userId),
                        new Category("Investimento", "trending_up", "#00cccc",BillTypeEnum.INCOME, userId),
                        new Category("Outros", "more_horiz", "#808080",BillTypeEnum.INCOME, userId),
                        new Category("Presente", "featured_seasonal_and_gifts", "#9dc209",BillTypeEnum.INCOME, userId),
                        new Category("Prêmio", "trophy", "#ffe135",BillTypeEnum.INCOME, userId),
                        new Category("Salário", "payments", "#8b0000",BillTypeEnum.INCOME, userId)
                    ]);

                    await _appDbContext.Categories.AddRangeAsync(initialCategories);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine($"Categorias iniciais incluidas no usuário id: {userId}");


                    await transaction.CommitAsync();

                    var userInfo = _mapper.Map<InfoUserResponse>(createdUser.Entity);

                    return userInfo;

                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
