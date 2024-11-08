using AutoMapper;
using ControleCerto.DTOs.Account;
using ControleCerto.DTOs.Events;
using ControleCerto.DTOs.User;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IHashService _hashService;
        private readonly IBus _bus;
        private readonly ICacheService _cacheService;
        public UserService(AppDbContext appDbContext, IMapper mapper, IHashService hashService, IBus bus, ICacheService cacheService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _hashService = hashService;
            _bus = bus;
            _cacheService = cacheService;
        }

        public async Task<Result<DetailsUserResponse>> GetUserDetails(int userId)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user is null) 
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            return _mapper.Map<DetailsUserResponse>(user);
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
                        new Category("Transporte", "local_taxi", "#00bfff", BillTypeEnum.EXPENSE, userId),
                        new Category("Vestuário", "checkroom", "#008b8b", BillTypeEnum.EXPENSE, userId),
                        new Category("Viagem", "travel", "#da70d6", BillTypeEnum.EXPENSE, userId),
                        new Category("Ajustes", "build", "ff5a1f", BillTypeEnum.EXPENSE, userId),
                        new Category("Investimento", "trending_up", "#00cccc",BillTypeEnum.INCOME, userId),
                        new Category("Outros", "more_horiz", "#808080",BillTypeEnum.INCOME, userId),
                        new Category("Presente", "featured_seasonal_and_gifts", "#9dc209",BillTypeEnum.INCOME, userId),
                        new Category("Prêmio", "trophy", "#ffe135",BillTypeEnum.INCOME, userId),
                        new Category("Salário", "payments", "#ae0c00",BillTypeEnum.INCOME, userId),
                        new Category("Ajustes", "build", "ff5a1f", BillTypeEnum.INCOME, userId),
                    ]);

                    await _appDbContext.Categories.AddRangeAsync(initialCategories);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine($"Categorias iniciais incluidas no usuário id: {userId}");

                    await transaction.CommitAsync();

                    var userInfo = _mapper.Map<InfoUserResponse>(createdUser.Entity);

                    await _bus.Publish(new ConfirmEmailEvent(userInfo));

                    return userInfo;

                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<Result<bool>> ConfirmEmailAsync(string token)
        {
            var email = await _cacheService.GetConfirmEmailTokenAsync(token);

            if (email is null) 
            {
                return new AppError("Token não encontrado, por favor, gere outro token.", ErrorTypeEnum.NotFound);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return new AppError("Nenhum usuário encontrado para esse email.", ErrorTypeEnum.NotFound);
            }

            user.EmailConfirmed = true;

            await _cacheService.RemoveConfirmEmailTokenAsync(token);

            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> GenerateConfirmEmailTokenAsync(int userId)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            if (user.EmailConfirmed is true)
            {
                return new AppError("Email já confirmado.", ErrorTypeEnum.Validation);
            }

            await _bus.Publish(new ConfirmEmailEvent(_mapper.Map<InfoUserResponse>(user)));

            return true;
        }

        public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, int userId)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            var oldPassHashed = _hashService.HashPassword(changePasswordRequest.OldPassword);

            if (user.PasswordHash != oldPassHashed)
            {
                return new AppError("Senha incorreta.", ErrorTypeEnum.Validation);
            }

            var newPassHashed = _hashService.HashPassword(changePasswordRequest.NewPassword);

            user.PasswordHash = newPassHashed;

            _appDbContext.Update(user);

            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<Result<bool>> GenerateForgotPasswordTokenAsync(string email)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return new AppError("E-mail não cadastrado no sistema.", ErrorTypeEnum.NotFound);
            }

            await _bus.Publish(new ForgotPasswordEvent(email));

            return true;
        }

        public async Task<Result<bool>> ForgotPasswordAsync(string token, ForgotPasswordRequest forgotPasswordRequest)
        {
            var email = await _cacheService.GetForgotPasswordTokenAsync(token);

            if (email is null)
            {
                return new AppError("Token não encontrado ou já utilizado.", ErrorTypeEnum.NotFound);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return new AppError("Nenhum usuário encontrado para esse email.", ErrorTypeEnum.NotFound);
            }

            if (forgotPasswordRequest.Password != forgotPasswordRequest.ConfirmPassword)
            {
                return new AppError("As senhas não conferem.", ErrorTypeEnum.NotFound);
            }

            var newPassHashed = _hashService.HashPassword(forgotPasswordRequest.Password);

            user.PasswordHash = newPassHashed;

            _appDbContext.Update(user);
            await _appDbContext.SaveChangesAsync();

            await _cacheService.RemoveForgotPasswordTokenAsync(token);

            return new Result<bool>(true);
        }

        public async Task<Result<bool>> VerifyForgotPasswordTokenAsync(string token)
        {
            var email = await _cacheService.GetForgotPasswordTokenAsync(token);

            if (email is null)
            {
                return new AppError("Token não encontrado, por favor, gere outro token.", ErrorTypeEnum.NotFound);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return new AppError("Nenhum usuário encontrado para esse email.", ErrorTypeEnum.NotFound);
            }

            return new Result<bool>(true);
        }

        public async Task<Result<DetailsUserResponse>> UpdateUserAsync(UpdateUserRequest userToUpdate, int userId)
        {
            if (userToUpdate.Id != userId)
            {
                return new AppError("Id no Bearer Token não condiz com o id do usuário a ser editado.", ErrorTypeEnum.BusinessRule);
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(a => a.Id == userToUpdate.Id);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.Validation);
            }

            if (userToUpdate.Name != null)
                user.Name = userToUpdate.Name;

            if (userToUpdate.Email != null)
            {
                user.Email = userToUpdate.Email;
                user.EmailConfirmed = false;
            }

            var updatedUser = _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<DetailsUserResponse>(updatedUser.Entity);
        }
    }
}
