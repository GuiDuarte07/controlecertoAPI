using AutoMapper;
using ControleCerto.DTOs.Article;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class ArticleService : IArticleService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        public ArticleService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<InfoArticleResponse>> GetArticleByTitleAsync(string title)
        {
            var article = await _appDbContext.Articles.FirstOrDefaultAsync(x => x.Title == title);

            if (article == null)
            {
                return new AppError("Artigo não encontrado", ErrorTypeEnum.NotFound);
            }

            return _mapper.Map<InfoArticleResponse>(article);
        }
    }
}
