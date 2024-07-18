using Finantech.DTOs.User;

namespace Finantech.Services.Interfaces
{
    public interface IUserService
    {
        public Task<InfoUserResponse> CreateUserAync(CreateUserRequest userReq);
    }
}
