using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsApp.ReadingLists;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers.ReadingLists
{
    [ApiController]
    [Route("api/saved-articles")]
    public class SavedArticleController : AbpControllerBase
    {
        private readonly ISavedArticleAppService _savedArticleAppService;

        public SavedArticleController(ISavedArticleAppService savedArticleAppService)
        {
            _savedArticleAppService = savedArticleAppService;
        }

        /// <summary>
        /// Gets saved articles for the current user
        /// </summary>
        [HttpGet("my-articles")]
        public async Task<List<SavedArticleDto>> GetMySavedArticlesAsync(
            [FromQuery] Guid? readingListId = null,
            [FromQuery] bool onlyUnread = false,
            [FromQuery] int maxCount = 100)
        {
            return await _savedArticleAppService.GetMySavedArticlesAsync(readingListId, onlyUnread, maxCount);
        }

        /// <summary>
        /// Gets a specific saved article
        /// </summary>
        [HttpGet("{id}")]
        public async Task<SavedArticleDto> GetSavedArticleAsync(Guid id)
        {
            return await _savedArticleAppService.GetSavedArticleAsync(id);
        }

        /// <summary>
        /// Checks if an article is already saved by the current user
        /// </summary>
        [HttpGet("is-saved")]
        public async Task<bool> IsArticleSavedAsync([FromQuery] string url)
        {
            return await _savedArticleAppService.IsArticleSavedAsync(url);
        }

        /// <summary>
        /// Saves an article for later reading
        /// </summary>
        [HttpPost("save")]
        public async Task<SavedArticleDto> SaveArticleAsync([FromBody] SaveArticleDto input)
        {
            return await _savedArticleAppService.SaveArticleAsync(input);
        }

        /// <summary>
        /// Updates a saved article (notes, read status, tags, etc.)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<SavedArticleDto> UpdateSavedArticleAsync(Guid id, [FromBody] UpdateSavedArticleDto input)
        {
            return await _savedArticleAppService.UpdateSavedArticleAsync(id, input);
        }

        /// <summary>
        /// Removes a saved article
        /// </summary>
        [HttpDelete("{id}")]
        public async Task UnsaveArticleAsync(Guid id)
        {
            await _savedArticleAppService.UnsaveArticleAsync(id);
        }

        /// <summary>
        /// Removes a saved article by URL
        /// </summary>
        [HttpDelete("by-url")]
        public async Task UnsaveArticleByUrlAsync([FromQuery] string url)
        {
            await _savedArticleAppService.UnsaveArticleByUrlAsync(url);
        }

        /// <summary>
        /// Mark an article as read
        /// </summary>
        [HttpPost("{id}/mark-as-read")]
        public async Task MarkAsReadAsync(Guid id)
        {
            await _savedArticleAppService.MarkAsReadAsync(id);
        }

        /// <summary>
        /// Mark an article as unread
        /// </summary>
        [HttpPost("{id}/mark-as-unread")]
        public async Task MarkAsUnreadAsync(Guid id)
        {
            await _savedArticleAppService.MarkAsUnreadAsync(id);
        }

        /// <summary>
        /// Move article to a different reading list
        /// </summary>
        [HttpPost("{articleId}/move-to-list/{readingListId?}")]
        public async Task MoveToReadingListAsync(Guid articleId, Guid? readingListId = null)
        {
            await _savedArticleAppService.MoveToReadingListAsync(articleId, readingListId);
        }

        /// <summary>
        /// Bulk update saved articles
        /// </summary>
        [HttpPost("bulk-update")]
        public async Task BulkUpdateSavedArticlesAsync([FromBody] BulkUpdateSavedArticlesDto input)
        {
            await _savedArticleAppService.BulkUpdateSavedArticlesAsync(input);
        }

        /// <summary>
        /// Get statistics about saved articles
        /// </summary>
        [HttpGet("stats")]
        public async Task<SavedArticleStatsDto> GetSavedArticleStatsAsync()
        {
            return await _savedArticleAppService.GetSavedArticleStatsAsync();
        }

        /// <summary>
        /// Gets the list of reading list IDs where an article is saved
        /// </summary>
        [HttpGet("lists-for-article")]
        public async Task<List<Guid>> GetReadingListIdsForArticleAsync([FromQuery] string url)
        {
            return await _savedArticleAppService.GetReadingListIdsForArticleAsync(url);
        }

        /// <summary>
        /// Removes a saved article from a specific reading list by URL
        /// </summary>
        [HttpDelete("by-url-and-list")]
        public async Task UnsaveArticleByUrlAndListAsync([FromQuery] string url, [FromQuery] Guid readingListId)
        {
            await _savedArticleAppService.UnsaveArticleByUrlAndListAsync(url, readingListId);
        }
    }
}