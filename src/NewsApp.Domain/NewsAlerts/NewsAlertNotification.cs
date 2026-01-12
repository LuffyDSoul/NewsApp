using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.NewsAlerts
{
    /// <summary>
    /// Represents a notification generated when new news are found for an alert list
    /// </summary>
    public class NewsAlertNotification : CreationAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        
        public Guid NewsAlertListId { get; set; }
        
        public string AlertListName { get; set; }
        
        public string Category { get; set; }
        
        public string LanguageCode { get; set; }
        
        /// <summary>
        /// Number of new articles found
        /// </summary>
        public int NewArticlesCount { get; set; }
        
        /// <summary>
        /// Whether the user has read this notification
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Whether an email was sent for this notification
        /// </summary>
        public bool EmailSent { get; set; }
        
        public DateTime? EmailSentAt { get; set; }
        
        /// <summary>
        /// Date of the newest article found
        /// </summary>
        public DateTime NewestArticleDate { get; set; }

        protected NewsAlertNotification()
        {
            // For EF Core
        }

        public NewsAlertNotification(
            Guid id,
            Guid userId,
            Guid newsAlertListId,
            string alertListName,
            string category,
            string languageCode,
            int newArticlesCount,
            DateTime newestArticleDate)
            : base(id)
        {
            UserId = userId;
            NewsAlertListId = newsAlertListId;
            AlertListName = alertListName;
            Category = category;
            LanguageCode = languageCode;
            NewArticlesCount = newArticlesCount;
            NewestArticleDate = newestArticleDate;
            IsRead = false;
            EmailSent = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }

        public void MarkEmailSent()
        {
            EmailSent = true;
            EmailSentAt = DateTime.UtcNow;
        }
    }
}
