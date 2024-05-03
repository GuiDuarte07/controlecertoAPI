using AutoMapper;
using Finantech.DTOs.Income;
using Finantech.DTOs.User;
using Finantech.Migrations;
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

        public async Task<InfoUserResponse> CreateUserAync(CreateUserRequest userReq)
        {
            var alreadyExistEmail = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == userReq.Email);

            Console.WriteLine(alreadyExistEmail);

            if (alreadyExistEmail != null) 
            {
                throw new Exception("Já existe uma conta com esse e-mail cadastrada!");
            }

            using (var transaction = _appDbContext.Database.BeginTransaction())
            {
                try
                {

                    var passHash = _hashService.HashPassword(userReq.Password);

                    var user = new User { Email = userReq.Email, Name = userReq.Name, PasswordHash = passHash };

                    var createdUser = _appDbContext.Users.Add(user);

                    await _appDbContext.SaveChangesAsync();

                    await _categoryService.SetAllDefaultCatogoriesAsync(createdUser.Entity.Id);

                    transaction.Commit();

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
