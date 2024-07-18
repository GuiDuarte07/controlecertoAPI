using Finantech.DTOs.Account;

namespace Finantech.DTOs.User
{
    public class InfoUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
