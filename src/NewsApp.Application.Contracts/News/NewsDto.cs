using System;
using Volo.Abp.Application.Dtos;

namespace NewsApp.News
{
    /// <summary>
    /// DTO for news articles
    /// </summary>
    public class NewsArticleDto : EntityDto<Guid>
    {
        /// <summary>
        /// Source of the news article
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Title of the news article
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description or summary of the article
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// URL to the full article
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL to the article's image
        /// </summary>
        public string? UrlToImage { get; set; }

        /// <summary>
        /// When the article was published
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Content of the article (if available)
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Language of the article
        /// </summary>
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Author of the article
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// When this article was created in our system
        /// </summary>
        public DateTime CreationTime { get; set; }
    }

    /// <summary>
    /// DTO for creating news articles
    /// </summary>
    public class CreateNewsArticleDto
    {
        public string Source { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? UrlToImage { get; set; }
        public DateTime PublishedAt { get; set; }
        public string? Content { get; set; }
        public string LanguageCode { get; set; } = "en";
        public string? Author { get; set; }
    }

    /// <summary>
    /// DTO for news search requests
    /// </summary>
    public class NewsSearchDto
    {
        public string Query { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "en";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Source { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "publishedAt"; // publishedAt, relevancy, popularity
    }

    /// <summary>
    /// DTO for news sources
    /// </summary>
    public class NewsSourceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    // Legacy DTO for backward compatibility
    [Obsolete("Use NewsArticleDto instead")]
    public class NewsDto
    {
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string UrlToImage { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
