using Finantech.DTOs.User;
using Finantech.Errors;

namespace Finantech.Services.Interfaces
{
    public interface IUserService
    {
        public Task<Result<InfoUserResponse>> CreateUserAync(CreateUserRequest userReq);
    }
}
