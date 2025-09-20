using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
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
        public async Task<ICollection<ArticleDto>> GetNewsAsync(string query)
        {
            ICollection<ArticleDto> responseList = new List<ArticleDto>();

            // init with your API key
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                // Mejorado: usar fecha más reciente para obtener noticias actuales
                From = DateTime.Now.AddDays(-7), // Últimos 7 días en lugar de 1 mes
                To = DateTime.Now, // Hasta hoy
                PageSize = 20 // Aumentado a 20 para más resultados
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
