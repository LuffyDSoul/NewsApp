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
            return await GetNewsAsync(query, language: null, from: DateTime.Now.AddDays(-7), to: DateTime.Now, pageSize: 20);
        }

        public async Task<ICollection<ArticleDto>> GetNewsAsync(
            string query, 
            string? language = null, 
            DateTime? from = null, 
            DateTime? to = null,
            int pageSize = 20)
        {
            ICollection<ArticleDto> responseList = new List<ArticleDto>();

            // init with API key from configuration
            var newsApiClient = new NewsApiClient(_newsApiKey);

            // Map language code to NewsAPI language enum
            Languages? apiLanguage = null;
            if (!string.IsNullOrEmpty(language))
            {
                apiLanguage = language.ToLower() switch
                {
                    "ar" => Languages.AR,
                    "de" => Languages.DE,
                    "en" => Languages.EN,
                    "es" => Languages.ES,
                    "fr" => Languages.FR,
                    "he" => Languages.HE,
                    "it" => Languages.IT,
                    "nl" => Languages.NL,
                    "no" => Languages.NO,
                    "pt" => Languages.PT,
                    "ru" => Languages.RU,
                    "sv" => Languages.SV,
                    "zh" => Languages.ZH,
                    _ => Languages.EN
                };
            }
            
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.PublishedAt, // Changed to PublishedAt to get most recent
                Language = apiLanguage ?? Languages.EN,
                From = from ?? DateTime.Now.AddDays(-1), // Default last 24 hours
                To = to ?? DateTime.Now,
                PageSize = pageSize
            });

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
                throw new InvalidOperationException($"NewsAPI error: Status = {articlesResponse.Status}");
            }
            
            return responseList;
        }
    }
}

