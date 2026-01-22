using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly INewsService _newsService;
        private readonly IConfiguration _configuration;
        private readonly string _newsApiKey;

        public NewsAppService(INewsService newsService, IConfiguration configuration)
        {
            _newsService = newsService;
            _configuration = configuration;
            _newsApiKey = _configuration["NewsApi:ApiKey"] ?? "";
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            var articles = new List<NewsArticleDto>();
            
            // Use /everything endpoint for better language support
            string query;
            
            if (string.IsNullOrEmpty(category))
            {
                // For "Latest News", use wildcard to get all news in the specified language
                query = "*";
            }
            else
            {
                // For specific categories, use the category name as query
                query = category;
            }

            Logger.LogInformation("Requesting news from NewsAPI (Query: {Query}, Language: {Language}, PageSize: {PageSize})", 
                query, language, pageSize);

            var everythingRequest = new EverythingRequest
            {
                Q = query,
                Language = GetLanguageFromCode(language),
                From = DateTime.UtcNow.AddDays(-7), // Last 7 days
                Page = page,
                PageSize = pageSize,
                SortBy = SortBys.PublishedAt // Most recent first
            };

            var response = await newsApiClient.GetEverythingAsync(everythingRequest);
            
            Logger.LogInformation("NewsAPI Response - Status: {Status}, TotalResults: {TotalResults}, ArticleCount: {ArticleCount}", 
                response.Status, response.TotalResults, response.Articles?.Count ?? 0);
            
            if (response.Status != Statuses.Ok)
            {
                Logger.LogWarning("NewsAPI returned non-OK status: {Status}. Error: {Error}", 
                    response.Status, response.Error?.Message ?? "Unknown error");
            }
            
            if (response.Status == Statuses.Ok && response.Articles != null)
            {
                articles = response.Articles.Select(a => new NewsArticleDto
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Limit page size to prevent crashes
            pageSize = Math.Min(pageSize, 10);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Sources = sources.Split(',').Select(s => s.Trim()).ToList(),
                Language = GetLanguageFromCode(language),
                From = DateTime.UtcNow.AddDays(-2),
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Use GetEverything to get a sample and extract sources
            var sampleRequest = new EverythingRequest
            {
                Q = "news",
                Language = GetLanguageFromCode(language ?? "en"),
                From = DateTime.UtcNow.AddDays(-7),
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Use top-headlines for latest news
            var headlinesRequest = new TopHeadlinesRequest
            {
                Country = Countries.US,
                PageSize = count
            };
            
            var response = await newsApiClient.GetTopHeadlinesAsync(headlinesRequest);

            var articles = new List<NewsArticleDto>();
            
            if (response.Status == Statuses.Ok && response.Articles != null)
            {
                articles = response.Articles.Take(count).Select(a => new NewsArticleDto
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Limit max result count to prevent crashes
            maxResultCount = Math.Min(maxResultCount, 10);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = source,
                Language = Languages.EN, // TODO: Add language parameter
                From = DateTime.UtcNow.AddDays(-2),
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
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Limit max result count to prevent crashes
            maxResultCount = Math.Min(maxResultCount, 10);
            
            // Build query with OR for multiple keywords
            var query = BuildKeywordQuery(searchText);
            
            var articles = await newsApiClient.GetEverythingAsync(new EverythingRequest
            {
                Q = query,
                Language = GetLanguageFromCode(languageCode ?? "en"),
                From = DateTime.UtcNow.AddDays(-3),
                PageSize = Math.Min(maxResultCount, 20)
            });

            var result = new List<NewsArticleDto>();
            
            if (articles.Status == Statuses.Ok && articles.Articles != null)
            {
                result = articles.Articles
                    .OrderByDescending(a => a.PublishedAt ?? DateTime.MinValue)
                    .Skip(skipCount)
                    .Take(maxResultCount)
                    .Select(a => new NewsArticleDto
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
                var newsApiClient = new NewsApiClient(_newsApiKey);
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

        public async Task<PagedResultDto<NewsArticleDto>> GetNewsWithFilterAsync(
            string query,
            string language = "en",
            int page = 1,
            int pageSize = 20)
        {
            var newsApiClient = new NewsApiClient(_newsApiKey);
            
            // Limit page size
            pageSize = Math.Min(pageSize, 20);
            
            var everythingRequest = new EverythingRequest
            {
                Q = query,
                Language = GetLanguageFromCode(language),
                From = DateTime.UtcNow.AddDays(-2),
                Page = page,
                PageSize = pageSize,
                SortBy = SortBys.PublishedAt
            };

            var result = await newsApiClient.GetEverythingAsync(everythingRequest);

            var articles = new List<NewsArticleDto>();
            
            if (result.Status == Statuses.Ok && result.Articles != null)
            {
                articles = result.Articles.Select(a => new NewsArticleDto
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

        /// <summary>
        /// Converts language code to NewsAPI Language constant
        /// </summary>
        private Languages GetLanguageFromCode(string languageCode)
        {
            return languageCode?.ToLower() switch
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
                _ => Languages.EN // Default to English
            };
        }

        /// <summary>
        /// Converts country code to NewsAPI Country constant
        /// </summary>
        private Countries GetCountryFromCode(string countryCode)
        {
            return countryCode?.ToLower() switch
            {
                "us" => Countries.US,
                "gb" => Countries.GB,
                "de" => Countries.DE,
                "fr" => Countries.FR,
                "it" => Countries.IT,
                _ => Countries.US // Default to US
            };
        }

        /// <summary>
        /// Maps category string to NewsAPI Category enum
        /// </summary>
        private Categories MapToNewsAPICategory(string category)
        {
            return category?.ToLower() switch
            {
                "business" => Categories.Business,
                "entertainment" => Categories.Entertainment,
                "health" => Categories.Health,
                "science" => Categories.Science,
                "sports" => Categories.Sports,
                "technology" => Categories.Technology,
                _ => Categories.Business // Default to Business since General doesn't exist
            };
        }

        /// <summary>
        /// Builds keyword query with OR for multiple keywords separated by comma or pipe
        /// </summary>
        private string BuildKeywordQuery(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return "news";
            }

            // Split by comma or pipe and trim whitespace
            var keywords = keyword.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToList();

            if (keywords.Count == 0)
            {
                return "news";
            }

            if (keywords.Count == 1)
            {
                return keywords[0];
            }

            // Build query with OR: "keyword1 OR keyword2 OR keyword3"
            return string.Join(" OR ", keywords);
        }
        
        /// <summary>
        /// Alias for BuildKeywordQuery - converts keywords to NewsAPI query format
        /// </summary>
        private string ConvertToNewsApiQuery(string keyword) => BuildKeywordQuery(keyword);
    }
}
