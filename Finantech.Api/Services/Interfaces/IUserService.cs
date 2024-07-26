using Finantech.DTOs.User;
using Finantech.Errors;

namespace Finantech.Services.Interfaces
{
    public interface IUserService
    {
        public Task<Result<InfoUserResponse>> CreateUserAync(CreateUserRequest userReq);
        public Task<Result<bool>> ConfirmEmail(string token);
        public Task<Result<bool>> GenerateConfirmEmailToken(int userId);
        public Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, int userId);
    }
}
