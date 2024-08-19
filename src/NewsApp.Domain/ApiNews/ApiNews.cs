using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NewsApp.ApiNews
{
    public class ApiNewsService : IApiNews
    {
        NewsApiClient newsApiClient;


       public async Task<ICollection<NewsArticleDto>> GetNewsArticleAsync(string? Search)
        {
            // init with your API key
            newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            ICollection<NewsArticleDto> responseList = new List<NewsArticleDto>();
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = "Apple",
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                From = new DateTime(2018, 1, 25),
                PageSize = 5 //PageSize es el límite de cantidad de noticias que queres que te busque
            });
            if (articlesResponse.Status == Statuses.Ok)
            {
                // total results found
                //Console.WriteLine(articlesResponse.TotalResults);
                // here's the first 20
                articlesResponse.Articles.ForEach(t => responseList.Add(new NewsArticleDto
                {
                    Author = t.Author,
                    Title = t.Title,
                    Description = t.Description,
                    Url = t.Url,
                    PublishedAt = (DateTime)t.PublishedAt,
                    UrlToImage = t.UrlToImage,
                    Content = t.Content
                }));
            }
            return responseList;
        }
    }
}
