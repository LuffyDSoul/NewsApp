using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;

namespace NewsApp.ReadingLists
{
    public class SavedArticle : Entity<Guid>
    {
        /// <summary>
        /// The user who saved this article
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Navigation property to User
        /// </summary>
        public virtual IdentityUser? User { get; set; }

        /// <summary>
        /// The reading list this article belongs to (optional)
        /// </summary>
        public Guid? ReadingListId { get; set; }
        
        /// <summary>
        /// Navigation property to ReadingList
        /// </summary>
        public virtual ReadingList? ReadingList { get; set; }

        /// <summary>
        /// Original source of the news article
        /// </summary>
        [MaxLength(256)]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Title of the news article
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Description or summary of the article
        /// </summary>
        [MaxLength(1024)]
        public string? Description { get; set; }

        /// <summary>
        /// URL to the full article
        /// </summary>
        [Required]
        [MaxLength(2048)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// URL to the article's image
        /// </summary>
        [MaxLength(2048)]
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
        [MaxLength(5)]
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Author of the article
        /// </summary>
        [MaxLength(256)]
        public string? Author { get; set; }

        /// <summary>
        /// When this article was saved by the user
        /// </summary>
        public DateTime SavedAt { get; set; }

        /// <summary>
        /// Whether the user has read this article
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// User's notes on this article
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Tags assigned to this article
        /// </summary>
        [MaxLength(1024)]
        public string? Tags { get; set; }

        public SavedArticle()
        {
            SavedAt = DateTime.UtcNow;
        }

        public SavedArticle(
            Guid userId,
            string title,
            string url,
            string? source = null,
            string? description = null,
            string? urlToImage = null,
            DateTime? publishedAt = null,
            string? content = null,
            string? author = null,
            Guid? readingListId = null)
        {
            UserId = userId;
            Title = title;
            Url = url;
            Source = source ?? string.Empty;
            Description = description;
            UrlToImage = urlToImage;
            PublishedAt = publishedAt;
            Content = content;
            Author = author;
            ReadingListId = readingListId;
            SavedAt = DateTime.UtcNow;
        }
    }
}