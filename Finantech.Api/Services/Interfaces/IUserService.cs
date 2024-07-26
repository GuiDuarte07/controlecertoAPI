using Finantech.DTOs.User;
using Finantech.Errors;

namespace Finantech.Services.Interfaces
{
    public interface IUserService
    {
        public Task<Result<InfoUserResponse>> CreateUserAync(CreateUserRequest userReq);
        public Task<Result<bool>> ConfirmEmailAsync(string token);
        public Task<Result<bool>> GenerateConfirmEmailTokenAsync(int userId);
        public Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, int userId);
        public Task<Result<bool>> GenerateForgotPasswordTokenAsync(string email);
        public Task<Result<bool>> ForgotPasswordAsync(string token, ForgotPasswordRequest forgotPasswordRequest);
        public Task<Result<bool>> VerifyForgotPasswordTokenAsync(string token);
    }
}
