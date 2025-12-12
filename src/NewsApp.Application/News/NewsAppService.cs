using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using NewsApp.Permissions;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace NewsApp.News
{
    /// <summary>
    /// Application service for news-related operations
    /// </summary>
    [Authorize]
    public class NewsAppService : NewsAppAppService, INewsAppService
    {
        private readonly INewsService _newsService; // Usar solo la implementación simple que funciona

        public NewsAppService(INewsService newsService)
        {
            _newsService = newsService;
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> SearchAsync(NewsSearchDto searchDto)
        {
            // Usar la implementación simple que funciona
            var legacyResult = await _newsService.GetNewsAsync(searchDto.Query);
            var articles = ObjectMapper.Map<ICollection<ArticleDto>, List<NewsArticleDto>>(legacyResult);
            
            return new PagedResultDto<NewsArticleDto>(
                articles.Count,
                articles);
        }

        [AllowAnonymous] // Temporal para pruebas
        public async Task<PagedResultDto<NewsArticleDto>> GetTopHeadlinesAsync(
            string? category = null,
            string? country = null,
            string language = "en",
            int page = 1,
            int pageSize = 10)
        {
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Limit page size to prevent crashes
            pageSize = Math.Min(pageSize, 10);
            
            // Build query based on category or use generic term
            string query = "news";
            if (!string.IsNullOrEmpty(category))
            {
                query = category;
            }
            
            var everythingRequest = new EverythingRequest
            {
                Q = query,
                Language = GetLanguageFromCode(language),
                Page = page,
                PageSize = pageSize,
                SortBy = SortBys.PublishedAt
            };

            var headlines = await newsApiClient.GetEverythingAsync(everythingRequest);

            var articles = new List<NewsArticleDto>();
            
            if (headlines.Status == Statuses.Ok && headlines.Articles != null)
            {
                articles = headlines.Articles.Select(a => new NewsArticleDto
                {
                    Id = Guid.NewGuid(),
                    Source = a.Source?.Name ?? "Unknown",
                    Title = a.Title ?? "",
                    Description = a.Description ?? "",
                    Url = a.Url ?? "",
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt ?? DateTime.Now,
                    Content = a.Content,
                    Author = a.Author,
                    LanguageCode = language
                }).ToList();
            }

            return new PagedResultDto<NewsArticleDto>(articles.Count, articles);
        }

        [AllowAnonymous] // Temporal para pruebas - Implementación simplificada
        public async Task<NewsArticleDto> GetAsync(Guid id)
        {
            // Para propósitos de prueba, retornar un artículo de ejemplo
            await Task.Delay(1); // Para hacer el método async
            
            return new NewsArticleDto
            {
                Id = id,
                Source = "Sample Source",
                Title = "Sample Article",
                Description = "This is a sample article for testing",
                Url = "https://example.com",
                PublishedAt = DateTime.Now,
                LanguageCode = "en",
                Author = "Sample Author"
            };
        }

        [AllowAnonymous] // Temporal para pruebas
        public async Task<PagedResultDto<NewsArticleDto>> GetFromSourcesAsync(
            string sources,
            string language = "en",
            int page = 1,
            int pageSize = 10)
        {
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Limit page size to prevent crashes
            pageSize = Math.Min(pageSize, 10);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Sources = sources.Split(',').Select(s => s.Trim()).ToList(),
                Language = GetLanguageFromCode(language),
                Page = page,
                PageSize = pageSize
            });

            var result = new List<NewsArticleDto>();
            
            if (articles.Status == Statuses.Ok && articles.Articles != null)
            {
                result = articles.Articles.Select(a => new NewsArticleDto
                {
                    Id = Guid.NewGuid(),
                    Source = a.Source?.Name ?? "Unknown",
                    Title = a.Title ?? "",
                    Description = a.Description ?? "",
                    Url = a.Url ?? "",
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt ?? DateTime.Now,
                    Content = a.Content,
                    Author = a.Author,
                    LanguageCode = language
                }).ToList();
            }

            return new PagedResultDto<NewsArticleDto>(result.Count, result);
        }

        [AllowAnonymous] // Temporal para pruebas
        public async Task<List<NewsSourceDto>> GetSourcesAsync(string? language = null, string? country = null)
        {
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Use GetEverything to get a sample and extract sources
            var sampleRequest = new EverythingRequest
            {
                Q = "news",
                Language = GetLanguageFromCode(language ?? "en"),
                PageSize = 20
            };

            var articles = await newsApiClient.GetEverythingAsync(sampleRequest);
            var sources = new List<NewsSourceDto>();

            if (articles.Status == Statuses.Ok && articles.Articles != null)
            {
                var uniqueSources = articles.Articles
                    .Where(a => a.Source != null)
                    .GroupBy(a => a.Source.Id)
                    .Select(g => g.First().Source)
                    .ToList();

                sources = uniqueSources.Select(s => new NewsSourceDto
                {
                    Id = s.Id ?? "",
                    Name = s.Name ?? "",
                    Description = "",
                    Url = "",
                    Category = "",
                    Language = language ?? "en",
                    Country = country ?? ""
                }).ToList();
            }

            return sources;
        }

        [AllowAnonymous] // Temporal para pruebas - Implementación simplificada
        public async Task<List<NewsArticleDto>> GetLatestAsync(int count = 10, string? languageCode = null)
        {
            // Para propósitos de prueba, usar la API en lugar del repositorio
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Limit count to prevent crashes
            count = Math.Min(count, 10);
            
            var headlines = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = "news",
                Language = GetLanguageFromCode(languageCode ?? "en"),
                PageSize = count,
                SortBy = SortBys.PublishedAt
            });

            var articles = new List<NewsArticleDto>();
            
            if (headlines.Status == Statuses.Ok && headlines.Articles != null)
            {
                articles = headlines.Articles.Take(count).Select(a => new NewsArticleDto
                {
                    Id = Guid.NewGuid(),
                    Source = a.Source?.Name ?? "Unknown",
                    Title = a.Title ?? "",
                    Description = a.Description ?? "",
                    Url = a.Url ?? "",
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt ?? DateTime.Now,
                    Content = a.Content,
                    Author = a.Author,
                    LanguageCode = languageCode ?? "en"
                }).ToList();
            }

            return articles;
        }

        [AllowAnonymous] // Temporal para pruebas - Implementación simplificada
        public async Task<NewsArticleDto> CreateAsync(CreateNewsArticleDto input)
        {
            // Para propósitos de prueba, simular la creación
            await Task.Delay(1);
            
            return new NewsArticleDto
            {
                Id = Guid.NewGuid(),
                Source = input.Source,
                Title = input.Title,
                Description = input.Description,
                Url = input.Url,
                UrlToImage = input.UrlToImage,
                PublishedAt = input.PublishedAt,
                Content = input.Content,
                Author = input.Author,
                LanguageCode = input.LanguageCode,
                CreationTime = DateTime.Now
            };
        }

        [AllowAnonymous] // Temporal para pruebas - Implementación simplificada
        public async Task<PagedResultDto<NewsArticleDto>> GetBySourceAsync(string source, int skipCount = 0, int maxResultCount = 10)
        {
            // Para propósitos de prueba, usar GetEverything con el source como query
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Limit max result count to prevent crashes
            maxResultCount = Math.Min(maxResultCount, 10);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = source,
                Language = Languages.EN, // TODO: Add language parameter
                PageSize = maxResultCount
            });

            var result = new List<NewsArticleDto>();
            
            if (articles.Status == Statuses.Ok && articles.Articles != null)
            {
                result = articles.Articles.Skip(skipCount).Take(maxResultCount).Select(a => new NewsArticleDto
                {
                    Id = Guid.NewGuid(),
                    Source = a.Source?.Name ?? "Unknown",
                    Title = a.Title ?? "",
                    Description = a.Description ?? "",
                    Url = a.Url ?? "",
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt ?? DateTime.Now,
                    Content = a.Content,
                    Author = a.Author,
                    LanguageCode = "en"
                }).ToList();
            }

            return new PagedResultDto<NewsArticleDto>(result.Count, result);
        }

        [AllowAnonymous] // Temporal para pruebas - Implementación simplificada
        public async Task<PagedResultDto<NewsArticleDto>> SearchLocalAsync(
            string searchText,
            string? languageCode = null,
            int skipCount = 0,
            int maxResultCount = 10)
        {
            // Para propósitos de prueba, usar la API directamente
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            
            // Limit max result count to prevent crashes
            maxResultCount = Math.Min(maxResultCount, 10);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = searchText,
                Language = GetLanguageFromCode(languageCode ?? "en"),
                PageSize = maxResultCount
            });

            var result = new List<NewsArticleDto>();
            
            if (articles.Status == Statuses.Ok && articles.Articles != null)
            {
                result = articles.Articles.Skip(skipCount).Take(maxResultCount).Select(a => new NewsArticleDto
                {
                    Id = Guid.NewGuid(),
                    Source = a.Source?.Name ?? "Unknown",
                    Title = a.Title ?? "",
                    Description = a.Description ?? "",
                    Url = a.Url ?? "",
                    UrlToImage = a.UrlToImage,
                    PublishedAt = a.PublishedAt ?? DateTime.Now,
                    Content = a.Content,
                    Author = a.Author,
                    LanguageCode = languageCode ?? "en"
                }).ToList();
            }

            return new PagedResultDto<NewsArticleDto>(result.Count, result);
        }

        [AllowAnonymous] // Temporal para pruebas
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
                var test = await newsApiClient.GetEverythingAsync(new EverythingRequest
                {
                    Q = "test",
                    PageSize = 1
                });
                return test.Status == Statuses.Ok;
            }
            catch
            {
                return false;
            }
        }

        [Obsolete("Use SearchAsync with NewsSearchDto instead")]
        public async Task<ICollection<NewsDto>> Search(string query)
        {
            var legacyResult = await _newsService.GetNewsAsync(query);
            return ObjectMapper.Map<ICollection<ArticleDto>, ICollection<NewsDto>>(legacyResult);
        }

        /// <summary>
        /// Converts language code to NewsAPI Language constant
        /// </summary>
        private Languages GetLanguageFromCode(string languageCode)
        {
            return languageCode?.ToLower() switch
            {
                "de" => Languages.DE,
                "en" => Languages.EN,
                "es" => Languages.ES,
                "fr" => Languages.FR,
                "he" => Languages.HE,
                "it" => Languages.IT,
                "nl" => Languages.NL,
                "no" => Languages.NO,
                "pt" => Languages.PT,
                "sv" => Languages.SV,
                _ => Languages.EN // Default to English
            };
        }
    }
}
