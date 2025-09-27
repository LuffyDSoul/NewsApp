using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewsApp.ReadingLists
{
    public interface ISavedArticleAppService : IApplicationService
    {
        /// <summary>
        /// Gets saved articles for the current user
        /// </summary>
        Task<List<SavedArticleDto>> GetMySavedArticlesAsync(
            Guid? readingListId = null, 
            bool onlyUnread = false, 
            int maxCount = 100);

        /// <summary>
        /// Gets a specific saved article
        /// </summary>
        Task<SavedArticleDto> GetSavedArticleAsync(Guid id);

        /// <summary>
        /// Checks if an article is already saved by the current user
        /// </summary>
        Task<bool> IsArticleSavedAsync(string url);

        /// <summary>
        /// Saves an article for later reading
        /// </summary>
        Task<SavedArticleDto> SaveArticleAsync(SaveArticleDto input);

        /// <summary>
        /// Updates a saved article (notes, read status, tags, etc.)
        /// </summary>
        Task<SavedArticleDto> UpdateSavedArticleAsync(Guid id, UpdateSavedArticleDto input);

        /// <summary>
        /// Removes a saved article
        /// </summary>
        Task UnsaveArticleAsync(Guid id);

        /// <summary>
        /// Removes a saved article by URL
        /// </summary>
        Task UnsaveArticleByUrlAsync(string url);

        /// <summary>
        /// Mark an article as read
        /// </summary>
        Task MarkAsReadAsync(Guid id);

        /// <summary>
        /// Mark an article as unread
        /// </summary>
        Task MarkAsUnreadAsync(Guid id);

        /// <summary>
        /// Move article to a different reading list
        /// </summary>
        Task MoveToReadingListAsync(Guid articleId, Guid? readingListId);

        /// <summary>
        /// Bulk update saved articles
        /// </summary>
        Task BulkUpdateSavedArticlesAsync(BulkUpdateSavedArticlesDto input);

        /// <summary>
        /// Get statistics about saved articles
        /// </summary>
        Task<SavedArticleStatsDto> GetSavedArticleStatsAsync();
    }

    /// <summary>
    /// Statistics about user's saved articles
    /// </summary>
    public class SavedArticleStatsDto
    {
        /// <summary>
        /// Total number of saved articles
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Number of unread articles
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// Number of articles saved this week
        /// </summary>
        public int SavedThisWeekCount { get; set; }

        /// <summary>
        /// Number of articles read this week
        /// </summary>
        public int ReadThisWeekCount { get; set; }
    }
}