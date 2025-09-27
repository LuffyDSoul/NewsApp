using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Volo.Abp.DependencyInjection;
using NewsApp.Domain.News;
using NewsApp.Domain.News.Services;
using NewsApp.Domain.Monitoring;
using NewsApp.Domain.Monitoring.Repositories;

namespace NewsApp.Infrastructure.News
{
    /// <summary>
    /// NewsAPI.org implementation of INewsProvider with resilience patterns
    /// </summary>
    public class NewsApiProvider : INewsProvider, ITransientDependency
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewsApiProvider> _logger;
        private readonly IApiCallMetricRepository _metricRepository;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        private const string BASE_URL = "https://newsapi.org/v2";
        private const string SEARCH_ENDPOINT = "/everything";
        private const string TOP_HEADLINES_ENDPOINT = "/top-headlines";
        private const string SOURCES_ENDPOINT = "/sources";

        public NewsApiProvider(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<NewsApiProvider> logger,
            IApiCallMetricRepository metricRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _metricRepository = metricRepository;

            // Configure base URL and default headers
            _httpClient.BaseAddress = new Uri(BASE_URL);
            var apiKey = _configuration["NewsApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("NewsAPI API key is not configured. Please set NewsApi:ApiKey in configuration.");
            }
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

            // Setup resilience policies
            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, duration, retryCount, context) =>
                    {
                        _logger.LogWarning("NewsAPI call failed, retrying in {Duration}ms. Attempt {RetryCount}/3", 
                            duration.TotalMilliseconds, retryCount);
                    })
                .WrapAsync(Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<HttpRequestException>()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: 5,
                        durationOfBreak: TimeSpan.FromMinutes(1),
                        onBreak: (exception, duration) =>
                        {
                            _logger.LogError("NewsAPI circuit breaker opened for {Duration}", duration);
                        },
                        onReset: () =>
                        {
                            _logger.LogInformation("NewsAPI circuit breaker closed");
                        }))
                .WrapAsync(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30)));
        }

        public async Task<IList<NewsArticle>> SearchAsync(string query, string language = "en", DateTime? from = null, int page = 1, int pageSize = 20)
        {
            var startTime = DateTime.UtcNow;
            var operation = "SearchNews";
            
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Search query cannot be empty", nameof(query));

                var parameters = new List<string>
                {
                    $"q={Uri.EscapeDataString(query)}",
                    $"language={language}",
                    $"page={page}",
                    $"pageSize={Math.Min(pageSize, 100)}", // NewsAPI max is 100
                    "sortBy=publishedAt"
                };

                if (from.HasValue)
                {
                    parameters.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
                }

                var endpoint = $"{SEARCH_ENDPOINT}?{string.Join("&", parameters)}";
                
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                var content = await response.Content.ReadAsStringAsync();
                
                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (newsResponse?.Status != "ok")
                {
                    throw new InvalidOperationException($"NewsAPI returned error: {newsResponse?.Message ?? "Unknown error"}");
                }

                return newsResponse.Articles?.Select(ConvertToNewsArticle).ToList() ?? new List<NewsArticle>();
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, SEARCH_ENDPOINT, startTime, ex);
                throw;
            }
        }

        public async Task<NewsSearchResult> SearchAsync(NewsSearchCriteria criteria)
        {
            var startTime = DateTime.UtcNow;
            var operation = "SearchNewsWithCriteria";
            
            try
            {
                if (string.IsNullOrWhiteSpace(criteria.Query))
                    throw new ArgumentException("Search query cannot be empty", nameof(criteria.Query));

                var parameters = new List<string>
                {
                    $"q={Uri.EscapeDataString(criteria.Query)}",
                    $"page={criteria.Page}",
                    $"pageSize={Math.Min(criteria.PageSize, 100)}", // NewsAPI max is 100
                    "sortBy=publishedAt"
                };

                if (!string.IsNullOrEmpty(criteria.Language))
                    parameters.Add($"language={criteria.Language}");

                if (criteria.Sources?.Any() == true)
                    parameters.Add($"sources={string.Join(",", criteria.Sources)}");

                if (criteria.From.HasValue)
                    parameters.Add($"from={criteria.From.Value:yyyy-MM-ddTHH:mm:ss}");

                if (criteria.To.HasValue)
                    parameters.Add($"to={criteria.To.Value:yyyy-MM-ddTHH:mm:ss}");

                if (!string.IsNullOrEmpty(criteria.SortBy))
                    parameters.Add($"sortBy={criteria.SortBy}");

                var endpoint = $"{SEARCH_ENDPOINT}?{string.Join("&", parameters)}";
                
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                var content = await response.Content.ReadAsStringAsync();
                
                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (newsResponse?.Status != "ok")
                {
                    return new NewsSearchResult
                    {
                        Success = false,
                        ErrorMessage = $"NewsAPI returned error: {newsResponse?.Message ?? "Unknown error"}",
                        Articles = new List<NewsArticle>(),
                        TotalResults = 0,
                        Page = criteria.Page,
                        PageSize = criteria.PageSize
                    };
                }

                var articles = newsResponse.Articles?.Select(ConvertToNewsArticle).ToList() ?? new List<NewsArticle>();

                return new NewsSearchResult
                {
                    Success = true,
                    Articles = articles,
                    TotalResults = newsResponse.TotalResults ?? articles.Count,
                    Page = criteria.Page,
                    PageSize = criteria.PageSize
                };
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, SEARCH_ENDPOINT, startTime, ex);
                
                return new NewsSearchResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Articles = new List<NewsArticle>(),
                    TotalResults = 0,
                    Page = criteria.Page,
                    PageSize = criteria.PageSize
                };
            }
        }

        public async Task<IList<NewsArticle>> GetTopHeadlinesAsync(string? category = null, string? country = null, string language = "en", int page = 1, int pageSize = 20)
        {
            var startTime = DateTime.UtcNow;
            var operation = "GetTopHeadlines";
            
            try
            {
                var parameters = new List<string>
                {
                    $"language={language}",
                    $"page={page}",
                    $"pageSize={Math.Min(pageSize, 100)}"
                };

                if (!string.IsNullOrEmpty(category))
                    parameters.Add($"category={category}");

                if (!string.IsNullOrEmpty(country))
                    parameters.Add($"country={country}");

                var endpoint = $"{TOP_HEADLINES_ENDPOINT}?{string.Join("&", parameters)}";
                
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                var content = await response.Content.ReadAsStringAsync();
                
                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (newsResponse?.Status != "ok")
                {
                    throw new InvalidOperationException($"NewsAPI returned error: {newsResponse?.Message ?? "Unknown error"}");
                }

                return newsResponse.Articles?.Select(ConvertToNewsArticle).ToList() ?? new List<NewsArticle>();
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, TOP_HEADLINES_ENDPOINT, startTime, ex);
                throw;
            }
        }

        public async Task<IList<NewsArticle>> GetFromSourcesAsync(string sources, string language = "en", int page = 1, int pageSize = 20)
        {
            var startTime = DateTime.UtcNow;
            var operation = "GetFromSources";
            
            try
            {
                if (string.IsNullOrWhiteSpace(sources))
                    throw new ArgumentException("Sources cannot be empty", nameof(sources));

                var parameters = new List<string>
                {
                    $"sources={Uri.EscapeDataString(sources)}",
                    $"page={page}",
                    $"pageSize={Math.Min(pageSize, 100)}"
                };

                var endpoint = $"{SEARCH_ENDPOINT}?{string.Join("&", parameters)}";
                
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                var content = await response.Content.ReadAsStringAsync();
                
                var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (newsResponse?.Status != "ok")
                {
                    throw new InvalidOperationException($"NewsAPI returned error: {newsResponse?.Message ?? "Unknown error"}");
                }

                return newsResponse.Articles?.Select(ConvertToNewsArticle).ToList() ?? new List<NewsArticle>();
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, SEARCH_ENDPOINT, startTime, ex);
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            var startTime = DateTime.UtcNow;
            var operation = "TestConnection";
            
            try
            {
                var endpoint = $"{SOURCES_ENDPOINT}?language=en&pageSize=1";
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, SOURCES_ENDPOINT, startTime, ex);
                return false;
            }
        }

        public async Task<IList<NewsSource>> GetSourcesAsync(string? language = null, string? country = null)
        {
            var startTime = DateTime.UtcNow;
            var operation = "GetSources";
            
            try
            {
                var parameters = new List<string>();

                if (!string.IsNullOrEmpty(language))
                    parameters.Add($"language={language}");

                if (!string.IsNullOrEmpty(country))
                    parameters.Add($"country={country}");

                var endpoint = parameters.Any() 
                    ? $"{SOURCES_ENDPOINT}?{string.Join("&", parameters)}"
                    : SOURCES_ENDPOINT;
                
                var response = await ExecuteWithMetricsAsync(operation, endpoint, startTime);
                var content = await response.Content.ReadAsStringAsync();
                
                var sourcesResponse = JsonSerializer.Deserialize<NewsApiSourcesResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (sourcesResponse?.Status != "ok")
                {
                    throw new InvalidOperationException($"NewsAPI returned error: {sourcesResponse?.Message ?? "Unknown error"}");
                }

                return sourcesResponse.Sources?.Select(s => new NewsSource
                {
                    Id = s.Id ?? "",
                    Name = s.Name ?? "",
                    Description = s.Description ?? "",
                    Url = s.Url ?? "",
                    Category = s.Category ?? "",
                    Language = s.Language ?? "",
                    Country = s.Country ?? ""
                }).ToList() ?? new List<NewsSource>();
            }
            catch (Exception ex)
            {
                await RecordFailureMetricAsync(operation, SOURCES_ENDPOINT, startTime, ex);
                throw;
            }
        }

        private async Task<HttpResponseMessage> ExecuteWithMetricsAsync(string operation, string endpoint, DateTime startTime)
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _httpClient.GetAsync(endpoint);
            });

            var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            
            var metric = ApiCallMetric.CreateSuccess(
                operation,
                endpoint,
                duration,
                (int)response.StatusCode,
                responseSizeBytes: response.Content.Headers.ContentLength);

            await _metricRepository.InsertAsync(metric);
            
            response.EnsureSuccessStatusCode();
            return response;
        }

        private async Task RecordFailureMetricAsync(string operation, string endpoint, DateTime startTime, Exception exception)
        {
            var duration = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var httpStatus = 0;
            
            if (exception is HttpRequestException httpEx && httpEx.Data.Contains("StatusCode"))
            {
                httpStatus = (int)httpEx.Data["StatusCode"]!;
            }

            var metric = ApiCallMetric.CreateFailure(
                operation,
                endpoint,
                duration,
                httpStatus,
                exception.Message);

            await _metricRepository.InsertAsync(metric);
        }

        private static NewsArticle ConvertToNewsArticle(NewsApiArticle apiArticle)
        {
            var article = new NewsArticle(
                Guid.NewGuid(),
                apiArticle.Source?.Name ?? "Unknown",
                apiArticle.Title ?? "",
                apiArticle.Url ?? "",
                apiArticle.PublishedAt ?? DateTime.UtcNow,
                "en", // Default language, could be improved by detection
                apiArticle.Description,
                apiArticle.UrlToImage,
                apiArticle.Content,
                apiArticle.Author);

            return article;
        }
    }

    // NewsAPI response models
    public class NewsApiResponse
    {
        public string? Status { get; set; }
        public int? TotalResults { get; set; }
        public List<NewsApiArticle>? Articles { get; set; }
        public string? Message { get; set; }
    }

    public class NewsApiArticle
    {
        public NewsApiSource? Source { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Content { get; set; }
    }

    public class NewsApiSource
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Category { get; set; }
        public string? Language { get; set; }
        public string? Country { get; set; }
    }

    public class NewsApiSourcesResponse
    {
        public string? Status { get; set; }
        public List<NewsApiSource>? Sources { get; set; }
        public string? Message { get; set; }
    }
}
