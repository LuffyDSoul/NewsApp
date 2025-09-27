using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;
using NewsApp.News;

namespace NewsApp.Domain.News.Events
{
    /// <summary>
    /// Domain event fired when new articles are detected that match certain criteria
    /// </summary>
    [EventName("NewsApp.NewArticlesDetected")]
    public class NewArticlesDetectedEvent : EtoBase
    {
        /// <summary>
        /// Search query that resulted in these articles
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Language code used in the search
        /// </summary>
        public string LanguageCode { get; set; } = string.Empty;

        /// <summary>
        /// List of article references that were found
        /// </summary>
        public List<ArticleReference> Articles { get; set; } = new();

        /// <summary>
        /// When the detection occurred
        /// </summary>
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Number of new articles found
        /// </summary>
        public int Count { get; set; }

        public NewArticlesDetectedEvent()
        {
            DetectedAt = DateTime.UtcNow;
        }

        public NewArticlesDetectedEvent(string query, string languageCode, List<ArticleReference> articles) : this()
        {
            Query = query;
            LanguageCode = languageCode;
            Articles = articles;
            Count = articles.Count;
        }
    }
}
