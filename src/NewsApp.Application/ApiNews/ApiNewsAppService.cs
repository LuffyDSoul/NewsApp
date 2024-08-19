using NewsApp.Articulos;
using NewsApp.ApiNews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;


namespace NewsApp.ApiNews
{
    public class ApiNewsAppService : NewsAppAppService, IApiNewsService
    {
        private readonly IApiNews _apiNews;
        public ApiNewsAppService(IApiNews apiNewsAppService)
        {
            _apiNews = apiNewsAppService;
        }
        public async Task<ICollection<ArticuloDto>> Search(string? Search)
        {
            var news = await _apiNews.GetNewsArticleAsync(Search);

            return ManualMapper.MapArticlesToNewsArticles(news);
        }
    }
}
