using ControleCerto.DTOs.Article;
using ControleCerto.Errors;

namespace ControleCerto.Services.Interfaces
{
    public interface IArticleService
    {
        public Task<Result<InfoArticleResponse>> GetArticleByTitleAsync(string title);
    }
}
