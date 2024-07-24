using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.ApiNews
{
    public class ApiService : IApiNewsService
    {
        private readonly NewsApiClient _newsApiClient;

        public ApiService()
        {
            _newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
        }

        public async Task<List<ArticuloDto>> GetNewsAsync(string query)
        {
            var articlesResponse = _newsApiClient.GetEverything(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                From = new DateTime(2018, 1, 25)
            });

            if (articlesResponse.Status == Statuses.Ok)
            {
                var articles = new List<ArticuloDto>();
                foreach (var article in articlesResponse.Articles)
                {
                    articles.Add(new ArticuloDto
                    {
                        Title = article.Title,
                        Author = article.Author,
                        Description = article.Description,
                        Url = article.Url,
                        UrlToImage = article.UrlToImage,
                        PublishedAt = article.PublishedAt,
                        Content = article.Content
                    });
                }
                return articles;
            }
            return new List<ArticuloDto>();
        }
    }
}
