using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Lists
{
    /// <summary>
    /// Application service for reading list operations
    /// </summary>
    public interface IReadingListAppService : IApplicationService
    {
        /// <summary>
        /// Get all reading lists for the current user
        /// </summary>
        /// <param name="includeHierarchy">Whether to include child lists</param>
        /// <returns>List of reading lists</returns>
        Task<List<ReadingListDto>> GetMyListsAsync(bool includeHierarchy = false);

        /// <summary>
        /// Get top-level reading lists for the current user
        /// </summary>
        /// <returns>Top-level reading lists</returns>
        Task<List<ReadingListHierarchyDto>> GetMyHierarchicalListsAsync();

        /// <summary>
        /// Get a specific reading list with its items
        /// </summary>
        /// <param name="id">Reading list ID</param>
        /// <param name="includeItems">Whether to include list items</param>
        /// <returns>Reading list with items</returns>
        Task<ReadingListWithItemsDto> GetWithItemsAsync(Guid id, bool includeItems = true);

        /// <summary>
        /// Create a new reading list
        /// </summary>
        /// <param name="input">Reading list data</param>
        /// <returns>Created reading list</returns>
        Task<ReadingListDto> CreateAsync(CreateReadingListDto input);

        /// <summary>
        /// Update an existing reading list
        /// </summary>
        /// <param name="id">Reading list ID</param>
        /// <param name="input">Updated reading list data</param>
        /// <returns>Updated reading list</returns>
        Task<ReadingListDto> UpdateAsync(Guid id, UpdateReadingListDto input);

        /// <summary>
        /// Delete a reading list
        /// </summary>
        /// <param name="id">Reading list ID</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Add an article to a reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="input">Article to add</param>
        /// <returns>Created reading list item</returns>
        Task<ReadingListItemDto> AddArticleAsync(Guid listId, AddArticleToListDto input);

        /// <summary>
        /// Remove an article from a reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="articleId">Article ID</param>
        Task RemoveArticleAsync(Guid listId, Guid articleId);

        /// <summary>
        /// Mark an article as read in a reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="articleId">Article ID</param>
        Task MarkAsReadAsync(Guid listId, Guid articleId);

        /// <summary>
        /// Mark an article as unread in a reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="articleId">Article ID</param>
        Task MarkAsUnreadAsync(Guid listId, Guid articleId);

        /// <summary>
        /// Update a reading list item
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="itemId">Reading list item ID</param>
        /// <param name="input">Updated item data</param>
        /// <returns>Updated reading list item</returns>
        Task<ReadingListItemDto> UpdateItemAsync(Guid listId, Guid itemId, UpdateReadingListItemDto input);

        /// <summary>
        /// Get child reading lists for a parent list
        /// </summary>
        /// <param name="parentId">Parent list ID</param>
        /// <returns>Child reading lists</returns>
        Task<List<ReadingListDto>> GetChildrenAsync(Guid parentId);

        /// <summary>
        /// Move a reading list to a different parent
        /// </summary>
        /// <param name="listId">List ID to move</param>
        /// <param name="newParentId">New parent list ID (null for root level)</param>
        Task MoveListAsync(Guid listId, Guid? newParentId);

        /// <summary>
        /// Get reading lists that contain a specific article
        /// </summary>
        /// <param name="articleId">Article ID</param>
        /// <returns>Lists containing the article</returns>
        Task<List<ReadingListDto>> GetListsContainingArticleAsync(Guid articleId);

        /// <summary>
        /// Search reading lists by name
        /// </summary>
        /// <param name="namePattern">Name pattern to search for</param>
        /// <returns>Matching reading lists</returns>
        Task<List<ReadingListDto>> SearchByNameAsync(string namePattern);

        /// <summary>
        /// Get public reading lists
        /// </summary>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Public reading lists</returns>
        Task<PagedResultDto<ReadingListDto>> GetPublicListsAsync(int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get recently updated reading lists for the current user
        /// </summary>
        /// <param name="count">Number of lists to return</param>
        /// <returns>Recently updated reading lists</returns>
        Task<List<ReadingListDto>> GetRecentlyUpdatedAsync(int count = 10);

        /// <summary>
        /// Get the full hierarchy path for a reading list
        /// </summary>
        /// <param name="listId">List ID</param>
        /// <returns>Hierarchy path from root to the specified list</returns>
        Task<List<ReadingListDto>> GetHierarchyPathAsync(Guid listId);

        /// <summary>
        /// Import articles to a reading list from a URL or file
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="importData">Import data (URLs, OPML, etc.)</param>
        /// <returns>Number of articles imported</returns>
        Task<int> ImportArticlesAsync(Guid listId, string importData);

        /// <summary>
        /// Export reading list to various formats
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <param name="format">Export format (json, csv, opml)</param>
        /// <returns>Exported data</returns>
        Task<string> ExportListAsync(Guid listId, string format = "json");
    }
}
