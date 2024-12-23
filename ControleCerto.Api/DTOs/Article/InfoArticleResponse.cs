using ControleCerto.DTOs.User;

namespace ControleCerto.DTOs.Article
{
    public class InfoArticleResponse
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string MdFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public InfoUserResponse User { get; set; }
    }
}
