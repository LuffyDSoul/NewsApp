using System;
using System.Collections.Generic;

namespace NewsApp.Domain.News.Services
{
    /// <summary>
    /// Criteria for searching news articles
    /// </summary>
    public class NewsSearchCriteria
    {
        /// <summary>
        /// Search query string
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// News sources to search in
        /// </summary>
        public List<string>? Sources { get; set; }

        /// <summary>
        /// Categories to filter by
        /// </summary>
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Language code for articles
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Country code for articles
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Keywords to exclude from results
        /// </summary>
        public List<string>? ExcludeKeywords { get; set; }

        /// <summary>
        /// Start date for article search
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// End date for article search
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Maximum number of results to return
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Page number for pagination
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Sort order for results
        /// </summary>
        public string? SortBy { get; set; }
    }

    /// <summary>
    /// Result of news search operation
    /// </summary>
    public class NewsSearchResult
    {
        /// <summary>
        /// List of articles found
        /// </summary>
        public List<Domain.News.NewsArticle> Articles { get; set; } = new();

        /// <summary>
        /// Total number of articles available
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of results per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Whether the search was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if search failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}