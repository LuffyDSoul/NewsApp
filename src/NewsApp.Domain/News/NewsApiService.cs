using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Statuses = NewsAPI.Constants.Statuses;

namespace NewsApp.News
{
    public class NewsApiService : INewsService
    {
        private readonly string _newsApiKey;

        public NewsApiService(IConfiguration configuration)
        {
            _newsApiKey = configuration["NewsApi:ApiKey"] ?? "";
        }

        public async Task<ICollection<ArticleDto>> GetNewsAsync(string query)
        {
            ICollection<ArticleDto> responseList = new List<ArticleDto>();

            // init with your API key
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                From = DateTime.Now.AddDays(-2),
                PageSize = 20
            });

            // Mejorado: manejar errores apropiadamente
            if (articlesResponse.Status == Statuses.Ok)
            {
                if (articlesResponse.Articles != null && articlesResponse.Articles.Any())
                {
                    articlesResponse.Articles.ForEach(t => responseList.Add(new ArticleDto 
                    {  
                        Author = t.Author ?? "Unknown", 
                        Title = t.Title ?? "",
                        Description = t.Description ?? "",
                        Url = t.Url ?? "",
                        UrlToImage = t.UrlToImage ?? "",
                        PublishedAt = t.PublishedAt,
                        Content = t.Content ?? ""
                    }));
                }
            }
            else
            {
                // Log del error o manejo apropiado sin acceder a Message
                throw new InvalidOperationException($"NewsAPI error: Status = {articlesResponse.Status}");
            }
            
            return responseList;
        }
    }
}
