using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsApp.Domain.News;

namespace NewsApp.Domain.News.Services
{
    /// <summary>
    /// Interface for news providers - allows pluggable implementations
    /// </summary>
    public interface INewsProvider
    {
        /// <summary>
        /// Search for news articles based on a query
        /// </summary>
        /// <param name="query">Search terms</param>
        /// <param name="language">Language code (e.g., "en", "es")</param>
        /// <param name="from">Start date for articles (optional)</param>
        /// <param name="page">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of articles per page (default: 20)</param>
        /// <returns>List of news articles</returns>
        Task<IList<NewsArticle>> SearchAsync(
            string query,
            string language = "en",
            DateTime? from = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Search for news articles based on criteria
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>Search result with articles</returns>
        Task<NewsSearchResult> SearchAsync(NewsSearchCriteria criteria);

        /// <summary>
        /// Get top headlines for a specific category or country
        /// </summary>
        /// <param name="category">News category (optional)</param>
        /// <param name="country">Country code (optional)</param>
        /// <param name="language">Language code</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of articles per page</param>
        /// <returns>List of top headline articles</returns>
        Task<IList<NewsArticle>> GetTopHeadlinesAsync(
            string? category = null,
            string? country = null,
            string language = "en",
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Get articles from specific sources
        /// </summary>
        /// <param name="sources">Comma-separated list of source IDs</param>
        /// <param name="language">Language code</param>
        /// <param name="page">Page number for pagination</param>
        /// <param name="pageSize">Number of articles per page</param>
        /// <returns>List of articles from specified sources</returns>
        Task<IList<NewsArticle>> GetFromSourcesAsync(
            string sources,
            string language = "en",
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Test the connection to the news provider
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Get available news sources
        /// </summary>
        /// <param name="language">Language code</param>
        /// <param name="country">Country code</param>
        /// <returns>List of available sources</returns>
        Task<IList<NewsSource>> GetSourcesAsync(string? language = null, string? country = null);
    }

    /// <summary>
    /// Represents a news source
    /// </summary>
    public class NewsSource
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
