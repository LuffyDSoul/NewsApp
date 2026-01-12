using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.NewsAlerts
{
    /// <summary>
    /// Represents a news alert list configured by a user to receive automatic notifications
    /// </summary>
    public class NewsAlertList : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        
        public string Name { get; set; }
        
        public string? Description { get; set; }
        
        /// <summary>
        /// Comma-separated list of categories (business, entertainment, general, health, science, sports, technology)
        /// </summary>
        public string Categories { get; set; }
        
        /// <summary>
        /// Language code for news (ar, de, en, es, fr, he, it, nl, no, pt, ru, sv, ud, zh)
        /// </summary>
        public string LanguageCode { get; set; }
        
        /// <summary>
        /// Optional keyword to filter news by title
        /// </summary>
        public string? Keyword { get; set; }
        
        /// <summary>
        /// Whether this alert list is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Last time this alert was checked for new news
        /// </summary>
        public DateTime? LastCheckedAt { get; set; }
        
        /// <summary>
        /// Last time news were found for this alert
        /// </summary>
        public DateTime? LastNewsFoundAt { get; set; }

        protected NewsAlertList()
        {
            // For EF Core
        }

        public NewsAlertList(
            Guid id,
            Guid userId,
            string name,
            string categories,
            string languageCode,
            string? description = null,
            string? keyword = null,
            bool isActive = true)
            : base(id)
        {
            UserId = userId;
            Name = name;
            Categories = categories;
            LanguageCode = languageCode;
            Description = description;
            Keyword = keyword;
            IsActive = isActive;
        }

        public void Update(string name, string categories, string languageCode, string? description, string? keyword, bool isActive)
        {
            Name = name;
            Categories = categories;
            LanguageCode = languageCode;
            Description = description;
            Keyword = keyword;
            IsActive = isActive;
        }

        public void MarkAsChecked()
        {
            LastCheckedAt = DateTime.UtcNow;
        }

        public void MarkNewsFound()
        {
            LastNewsFoundAt = DateTime.UtcNow;
        }
    }
}
