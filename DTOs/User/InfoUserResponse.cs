namespace Finantech.DTOs.User
{
    public class InfoUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
