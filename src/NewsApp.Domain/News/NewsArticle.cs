using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using NewsApp.News;

namespace NewsApp.Domain.News
{
    /// <summary>
    /// Represents a news article entity with all relevant information
    /// </summary>
    public class NewsArticle : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// Source of the news article (e.g., "CNN", "BBC")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Title of the news article
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description or summary of the article
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// URL to the full article
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL to the article's image
        /// </summary>
        [MaxLength(2000)]
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
        [MaxLength(300)]
        public string? Author { get; set; }

        /// <summary>
        /// Hash to prevent duplicates
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string UrlHash { get; set; } = string.Empty;

        protected NewsArticle()
        {
            // For EF Core
        }

        public NewsArticle(
            Guid id,
            string source,
            string title,
            string url,
            DateTime publishedAt,
            string languageCode = "en",
            string? description = null,
            string? urlToImage = null,
            string? content = null,
            string? author = null) : base(id)
        {
            SetBasicInfo(source, title, url, publishedAt, languageCode);
            Description = description;
            UrlToImage = urlToImage;
            Content = content;
            Author = author;
        }

        public void SetBasicInfo(string source, string title, string url, DateTime publishedAt, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Source cannot be empty", nameof(source));
            
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));
            
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));

            Source = source;
            Title = title;
            Url = url;
            PublishedAt = publishedAt;
            LanguageCode = languageCode;
            UrlHash = ComputeUrlHash(url);
        }

        public ArticleReference ToReference()
        {
            return new ArticleReference(Url, Title, Source, PublishedAt);
        }

        private static string ComputeUrlHash(string url)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(url.ToLowerInvariant());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
