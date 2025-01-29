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
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");//Se cambio esto
            var articlesResponse = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = query,
                SortBy = SortBys.Popularity,
                Language = Languages.EN,
                // consultamos de un mes para atras ya que es lo que permite la api gratis
                From = DateTime.Now.AddMonths(-1),
                PageSize = 5 // Para no pasarnos del límite de la API
            }) ;

            //TODO: se deberia lanzar una excepcion si la consulta a la api da error.
            if (articlesResponse.Status == Statuses.Ok)
            {
                articlesResponse.Articles.ForEach( t=> responseList.Add(new ArticleDto {  Author = t.Author, 
                                                                                          Title = t.Title,
                                                                                          Description = t.Description,
                                                                                          Url = t.Url,
                                                                                          PublishedAt = t.PublishedAt
                }));                                
            }
            
            return responseList;
        }
    }
}
