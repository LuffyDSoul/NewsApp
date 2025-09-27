using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using NewsApp.Domain.News;

namespace NewsApp.Domain.News.Repositories
{
    /// <summary>
    /// Repository interface for NewsArticle entities
    /// </summary>
    public interface INewsArticleRepository : IRepository<NewsArticle, Guid>
    {
        /// <summary>
        /// Count articles in a specific time period
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Count of articles in the specified period</returns>
        Task<int> CountInPeriodAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Find article by URL hash to prevent duplicates
        /// </summary>
        /// <param name="urlHash">Hash of the article URL</param>
        /// <returns>Article if found, null otherwise</returns>
        Task<NewsArticle?> FindByUrlHashAsync(string urlHash);

        /// <summary>
        /// Get articles by source
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>List of articles from the specified source</returns>
        Task<List<NewsArticle>> GetBySourceAsync(string source, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get articles published within a date range
        /// </summary>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>List of articles published within the date range</returns>
        Task<List<NewsArticle>> GetByDateRangeAsync(DateTime from, DateTime to, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Search articles by title and description content
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="languageCode">Language code filter</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>List of articles matching the search criteria</returns>
        Task<List<NewsArticle>> SearchAsync(string searchText, string? languageCode = null, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get articles by language
        /// </summary>
        /// <param name="languageCode">Language code</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>List of articles in the specified language</returns>
        Task<List<NewsArticle>> GetByLanguageAsync(string languageCode, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get latest articles
        /// </summary>
        /// <param name="count">Number of articles to return</param>
        /// <param name="languageCode">Optional language filter</param>
        /// <returns>Latest articles ordered by published date</returns>
        Task<List<NewsArticle>> GetLatestAsync(int count = 10, string? languageCode = null);

        /// <summary>
        /// Get articles that match multiple URLs (for bulk operations)
        /// </summary>
        /// <param name="urls">List of URLs to match</param>
        /// <returns>Articles that match any of the provided URLs</returns>
        Task<List<NewsArticle>> GetByUrlsAsync(List<string> urls);

        /// <summary>
        /// Check if articles with the given URL hashes already exist
        /// </summary>
        /// <param name="urlHashes">List of URL hashes to check</param>
        /// <returns>Dictionary mapping URL hash to whether it exists</returns>
        Task<Dictionary<string, bool>> CheckExistenceByUrlHashesAsync(List<string> urlHashes);
    }
}
