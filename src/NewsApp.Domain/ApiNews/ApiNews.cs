using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public class ApiNewsService : IApiNews
    {
        private readonly NewsApiClient _newsApiClient;

        public ApiNewsService()
        {
            _newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
        }

        public async Task<ICollection<NewsArticleDto>> GetArticulosAsync(string? Search)
        {
            ICollection<NewsArticleDto> responseList = new List<NewsArticleDto>();

            var articlesResponse = _newsApiClient.GetEverything(new EverythingRequest
            {
                Q = Search ?? "Apple", //Si no hay parametro de entrada se buscan las noticias relacionadas con Apple
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                From = new DateTime(2018, 1, 25),
                PageSize = 5 // Para no pasarnos del límite de la API
            });

            if (articlesResponse.Status == Statuses.Ok)
            {
                articlesResponse.Articles.ForEach(t => responseList.Add(new NewsArticleDto
                {
                    Title = t.Title,
                    Author = t.Author,
                    Description = t.Description,
                    Url = t.Url,
                    PublishedAt = t.PublishedAt,
                    UrlToImage = t.UrlToImage,
                    Content = t.Content
                }));
                return responseList;
            }
            else
            {
                throw new ApplicationException("No se pudieron obtener noticias desde NewsAPI, error: " + articlesResponse.Error.Code);
            }
        }
    }
}
