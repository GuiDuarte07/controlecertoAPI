namespace ControleCerto.Models.Entities
{
    public class Article
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string MdFileName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
