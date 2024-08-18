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

            return ManualMapper.MapArticlesToNews(news);
        }

        public async Task<ICollection<ArticuloDto>> SearchFromDate(string? Search, DateTime date)
        {
            // Este código sería en el caso que busquemos noticias desde la API, pero necesitamos simular (para el testing) que encontramos una noticia nueva
            // así que utilizaremos nuestro propio repositorio de noticias, donde se agregó una nueva el día después de la creación de la alerta
            var news = await _apiNews.GetNewsArticleAsync(Search);
            var newsFromDate = new List<NewsArticleDto>();

            foreach (NewsArticleDto newD in news)
            {
                if (newD.PublishedAt >= date)
                {
                    newsFromDate.Add(newD);
                }
            };

            return ManualMapper.MapArticlesToNews(newsFromDate);
        }


    }
}
