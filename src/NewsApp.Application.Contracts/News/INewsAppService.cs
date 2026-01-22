using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.News
{
    /// <summary>
    /// Application service for news-related operations
    /// </summary>
    public interface INewsAppService : IApplicationService
    {
        /// <summary>
        /// Search for news articles
        /// </summary>
        /// <param name="searchDto">Search parameters</param>
        /// <returns>Paginated list of news articles</returns>
        Task<PagedResultDto<NewsArticleDto>> SearchAsync(NewsSearchDto searchDto);

        /// <summary>
        /// Get top headlines
        /// </summary>
        /// <param name="category">News category (optional)</param>
        /// <param name="country">Country code (optional)</param>
        /// <param name="language">Language code</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of articles per page</param>
        /// <returns>Paginated list of top headlines</returns>
        Task<PagedResultDto<NewsArticleDto>> GetTopHeadlinesAsync(
            string? category = null, 
            string? country = null, 
            string language = "en", 
            int page = 1, 
            int pageSize = 20);

        /// <summary>
        /// Get a specific news article by ID
        /// </summary>
        /// <param name="id">Article ID</param>
        /// <returns>News article details</returns>
        Task<NewsArticleDto> GetAsync(Guid id);

        /// <summary>
        /// Get articles from specific sources
        /// </summary>
        /// <param name="sources">Comma-separated list of source IDs</param>
        /// <param name="language">Language code</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of articles per page</param>
        /// <returns>Paginated list of articles from specified sources</returns>
        Task<PagedResultDto<NewsArticleDto>> GetFromSourcesAsync(
            string sources, 
            string language = "en", 
            int page = 1, 
            int pageSize = 20);

        /// <summary>
        /// Get available news sources
        /// </summary>
        /// <param name="language">Language code (optional)</param>
        /// <param name="country">Country code (optional)</param>
        /// <returns>List of available news sources</returns>
        Task<List<NewsSourceDto>> GetSourcesAsync(string? language = null, string? country = null);

        /// <summary>
        /// Get latest articles (from local database)
        /// </summary>
        /// <param name="count">Number of articles to return</param>
        /// <param name="languageCode">Language filter (optional)</param>
        /// <returns>Latest articles</returns>
        Task<List<NewsArticleDto>> GetLatestAsync(int count = 10, string? languageCode = null);

        /// <summary>
        /// Save an article to the local database
        /// </summary>
        /// <param name="input">Article data</param>
        /// <returns>Created article</returns>
        Task<NewsArticleDto> CreateAsync(CreateNewsArticleDto input);

        /// <summary>
        /// Get articles by source
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Articles from the specified source</returns>
        Task<PagedResultDto<NewsArticleDto>> GetBySourceAsync(string source, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Search articles in the local database
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="languageCode">Language filter (optional)</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Articles matching the search criteria</returns>
        Task<PagedResultDto<NewsArticleDto>> SearchLocalAsync(
            string searchText, 
            string? languageCode = null, 
            int skipCount = 0, 
            int maxResultCount = 10);

        /// <summary>
        /// Test connection to news provider
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Get news with filter (for background workers and alerts)
        /// </summary>
        /// <param name="query">Search query (category name or keywords)</param>
        /// <param name="language">Language code</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of articles per page</param>
        /// <returns>Paginated list of articles</returns>
        Task<PagedResultDto<NewsArticleDto>> GetNewsWithFilterAsync(
            string query,
            string language = "en",
            int page = 1,
            int pageSize = 20);

        // Legacy method for backward compatibility
//        [Obsolete("Use SearchAsync with NewsSearchDto instead")]
//        Task<ICollection<NewsDto>> Search(string query);
    }
}
