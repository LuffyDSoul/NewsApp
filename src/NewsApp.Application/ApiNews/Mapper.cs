using NewsApp.ApiNews;
using NewsApp.Articulos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public class ManualMapper
    {
        public static ICollection<ArticuloDto> MapArticlesToNewsArticles(ICollection<NewsArticleDto> articles)
        {
            var newsList = new List<ArticuloDto>();

            foreach (var article in articles)
            {
                var newsDto = new ArticuloDto
                {
                    Author = article.Author,
                    Title = article.Title,
                    Description = article.Description,
                    Content = article.Content,
                    PublishedAt = article.PublishedAt,
                    Url = article.Url,
                    UrlToImage = article.UrlToImage
                };

                newsList.Add(newsDto);
            }

            return newsList;
        }
    }
}
