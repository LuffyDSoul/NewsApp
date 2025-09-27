using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using NewsApp.News;

namespace NewsApp.Domain.Lists
{
    /// <summary>
    /// Represents an item in a reading list - a reference to a news article
    /// </summary>
    public class ReadingListItem : AuditedEntity<Guid>
    {
        /// <summary>
        /// ID of the reading list this item belongs to
        /// </summary>
        public Guid ReadingListId { get; set; }

        /// <summary>
        /// ID of the referenced article
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// Snapshot of article title at the time of adding
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ArticleTitle { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot of article URL at the time of adding
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string ArticleUrl { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot of article source at the time of adding
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string ArticleSource { get; set; } = string.Empty;

        /// <summary>
        /// When the article was published
        /// </summary>
        public DateTime ArticlePublishedAt { get; set; }

        /// <summary>
        /// Whether this article has been read by the user
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When this item was added to the reading list
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// When this article was marked as read (null if not read)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// User's personal notes about this article
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// User's rating for this article (1-5 stars)
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Navigation property to the reading list
        /// </summary>
        public virtual ReadingList ReadingList { get; set; } = null!;

        protected ReadingListItem()
        {
            // For EF Core
        }

        public ReadingListItem(
            Guid id,
            Guid readingListId,
            Guid articleId,
            string articleTitle,
            string articleUrl,
            string articleSource,
            DateTime articlePublishedAt) : base(id)
        {
            SetArticleInfo(readingListId, articleId, articleTitle, articleUrl, articleSource, articlePublishedAt);
            AddedAt = DateTime.UtcNow;
        }

        public void SetArticleInfo(
            Guid readingListId,
            Guid articleId,
            string articleTitle,
            string articleUrl,
            string articleSource,
            DateTime articlePublishedAt)
        {
            if (readingListId == Guid.Empty)
                throw new ArgumentException("Reading list ID cannot be empty", nameof(readingListId));

            if (articleId == Guid.Empty)
                throw new ArgumentException("Article ID cannot be empty", nameof(articleId));

            if (string.IsNullOrWhiteSpace(articleTitle))
                throw new ArgumentException("Article title cannot be empty", nameof(articleTitle));

            if (string.IsNullOrWhiteSpace(articleUrl))
                throw new ArgumentException("Article URL cannot be empty", nameof(articleUrl));

            if (string.IsNullOrWhiteSpace(articleSource))
                throw new ArgumentException("Article source cannot be empty", nameof(articleSource));

            ReadingListId = readingListId;
            ArticleId = articleId;
            ArticleTitle = articleTitle;
            ArticleUrl = articleUrl;
            ArticleSource = articleSource;
            ArticlePublishedAt = articlePublishedAt;
        }

        public void MarkAsRead(DateTime? readAt = null)
        {
            IsRead = true;
            ReadAt = readAt ?? DateTime.UtcNow;
        }

        public void MarkAsUnread()
        {
            IsRead = false;
            ReadAt = null;
        }

        public void SetRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

            Rating = rating;
        }

        public void SetNotes(string? notes)
        {
            Notes = notes;
        }

        public ArticleReference ToArticleReference()
        {
            return new ArticleReference(ArticleUrl, ArticleTitle, ArticleSource, ArticlePublishedAt);
        }
    }
}
