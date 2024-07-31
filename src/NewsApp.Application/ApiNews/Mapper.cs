using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.ApiNews
{
    public class ManualMapper
    {
        public static ICollection<Article> MapArticlesToNews(ICollection<ArticuloDto> articles)
        {
            var newsList = new List<Article>();

            foreach (var article in articles)
            {
                var articuloDto = new Article
                {
                    Author = article.Author,
                    Title = article.Title,
                    Description = article.Description,
                    Content = article.Content,
                    PublishedAt = article.PublishedAt,
                    Url = article.Url,
                    UrlToImage = article.UrlToImage
                };

                newsList.Add(articuloDto);
            }

            return newsList;
        }
    }
}
