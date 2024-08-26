using ControleCerto.DTOs.User;

namespace ControleCerto.DTOs.Auth
{
    public class AuthResponse
    {
        public InfoUserResponse User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken {  get; set; }
    }
}
