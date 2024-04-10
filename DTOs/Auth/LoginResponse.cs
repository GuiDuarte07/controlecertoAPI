using Finantech.DTOs.User;

namespace Finantech.DTOs.Auth
{
    public class LoginResponse
    {
        public InfoUserResponse User { get; set; }
        public string token { get; set; }
    }
}
