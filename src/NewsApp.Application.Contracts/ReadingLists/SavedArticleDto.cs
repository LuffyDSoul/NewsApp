using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace NewsApp.ReadingLists
{
    /// <summary>
    /// DTO for saved article information
    /// </summary>
    public class SavedArticleDto : EntityDto<Guid>
    {
        /// <summary>
        /// The reading list this article belongs to (optional)
        /// </summary>
        public Guid? ReadingListId { get; set; }

        /// <summary>
        /// Name of the reading list (if any)
        /// </summary>
        public string? ReadingListName { get; set; }

        /// <summary>
        /// Original source of the news article
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
        /// When the original article was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }

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
        /// When this article was saved by the user
        /// </summary>
        public DateTime SavedAt { get; set; }

        /// <summary>
        /// Whether the user has read this article
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// User's notes on this article
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Tags assigned to this article
        /// </summary>
        public string? Tags { get; set; }
    }

    /// <summary>
    /// DTO for saving an article
    /// </summary>
    public class SaveArticleDto
    {
        /// <summary>
        /// The reading list to save this article to (optional)
        /// </summary>
        public Guid? ReadingListId { get; set; }

        /// <summary>
        /// Original source of the news article
        /// </summary>
        [StringLength(256)]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Title of the news article
        /// </summary>
        [Required]
        [StringLength(512)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description or summary of the article
        /// </summary>
        [StringLength(1024)]
        public string? Description { get; set; }

        /// <summary>
        /// URL to the full article
        /// </summary>
        [Required]
        [StringLength(2048)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL to the article's image
        /// </summary>
        [StringLength(2048)]
        public string? UrlToImage { get; set; }

        /// <summary>
        /// When the original article was published
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Content of the article (if available)
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Language of the article
        /// </summary>
        [StringLength(5)]
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Author of the article
        /// </summary>
        [StringLength(256)]
        public string? Author { get; set; }

        /// <summary>
        /// User's notes on this article
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Tags assigned to this article
        /// </summary>
        [StringLength(1024)]
        public string? Tags { get; set; }
    }

    /// <summary>
    /// DTO for updating a saved article
    /// </summary>
    public class UpdateSavedArticleDto
    {
        /// <summary>
        /// The reading list this article belongs to (optional)
        /// </summary>
        public Guid? ReadingListId { get; set; }

        /// <summary>
        /// Whether the user has read this article
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// User's notes on this article
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Tags assigned to this article
        /// </summary>
        [StringLength(1024)]
        public string? Tags { get; set; }
    }

    /// <summary>
    /// DTO for bulk operations on saved articles
    /// </summary>
    public class BulkUpdateSavedArticlesDto
    {
        /// <summary>
        /// List of article IDs to update
        /// </summary>
        [Required]
        public List<Guid> ArticleIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Mark as read/unread (optional)
        /// </summary>
        public bool? MarkAsRead { get; set; }

        /// <summary>
        /// Move to reading list (optional)
        /// </summary>
        public Guid? MoveToReadingListId { get; set; }

        /// <summary>
        /// Add tags (optional)
        /// </summary>
        public string? AddTags { get; set; }

        /// <summary>
        /// Remove tags (optional)
        /// </summary>
        public string? RemoveTags { get; set; }
    }
}