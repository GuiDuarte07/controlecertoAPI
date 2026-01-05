namespace ControleCerto.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Account> Accounts { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Article> Articles { get; set; }
    }
}
