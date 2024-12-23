using ControleCerto.Extensions;
using ControleCerto.Services;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [AllowAnonymous]
        [HttpGet("GetArticleByTitle/{title}")]
        public async Task<IActionResult> GetArticleByTitle([FromRoute] string title)
        {
            var article = await _articleService.GetArticleByTitleAsync(title);

            return article.HandleReturnResult();
        }
    }
}
