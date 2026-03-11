using ControleCerto.Extensions;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetArticleByTitle([FromQuery] string title)
        {
            var article = await _articleService.GetArticleByTitleAsync(title);

            return article.HandleReturnResult();
        }
    }
}
