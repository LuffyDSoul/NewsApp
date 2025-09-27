using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using NewsApp.Domain.Lists;

namespace NewsApp.Domain.Lists.Repositories
{
    /// <summary>
    /// Repository interface for ReadingList entities
    /// </summary>
    public interface IReadingListRepository : IRepository<ReadingList, Guid>
    {
        /// <summary>
        /// Count reading lists with a specific condition
        /// </summary>
        /// <param name="predicate">Condition to filter the count</param>
        /// <returns>Count of reading lists matching the condition</returns>
        Task<int> CountAsync(Expression<Func<ReadingList, bool>> predicate);

        /// <summary>
        /// Get all reading lists for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeItems">Whether to include list items</param>
        /// <returns>List of reading lists owned by the user</returns>
        Task<List<ReadingList>> GetByUserIdAsync(Guid userId, bool includeItems = false);

        /// <summary>
        /// Get top-level reading lists for a user (lists without parent)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeChildren">Whether to include child lists</param>
        /// <param name="includeItems">Whether to include list items</param>
        /// <returns>Top-level reading lists</returns>
        Task<List<ReadingList>> GetTopLevelByUserIdAsync(Guid userId, bool includeChildren = false, bool includeItems = false);

        /// <summary>
        /// Get child reading lists for a specific parent list
        /// </summary>
        /// <param name="parentId">Parent list ID</param>
        /// <param name="includeItems">Whether to include list items</param>
        /// <returns>Child reading lists</returns>
        Task<List<ReadingList>> GetChildrenAsync(Guid parentId, bool includeItems = false);

        /// <summary>
        /// Get reading list with all its items
        /// </summary>
        /// <param name="listId">List ID</param>
        /// <param name="userId">User ID (for authorization)</param>
        /// <returns>Reading list with items, or null if not found or not accessible</returns>
        Task<ReadingList?> GetWithItemsAsync(Guid listId, Guid userId);

        /// <summary>
        /// Find lists that contain a specific article
        /// </summary>
        /// <param name="articleId">Article ID</param>
        /// <param name="userId">User ID (optional, to filter by user)</param>
        /// <returns>Lists containing the specified article</returns>
        Task<List<ReadingList>> FindListsContainingArticleAsync(Guid articleId, Guid? userId = null);

        /// <summary>
        /// Get reading lists by name pattern
        /// </summary>
        /// <param name="namePattern">Name pattern to search for</param>
        /// <param name="userId">User ID</param>
        /// <returns>Lists matching the name pattern</returns>
        Task<List<ReadingList>> GetByNamePatternAsync(string namePattern, Guid userId);

        /// <summary>
        /// Get public reading lists
        /// </summary>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Public reading lists</returns>
        Task<List<ReadingList>> GetPublicListsAsync(int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get recently updated reading lists for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="count">Number of lists to return</param>
        /// <returns>Recently updated reading lists</returns>
        Task<List<ReadingList>> GetRecentlyUpdatedAsync(Guid userId, int count = 10);

        /// <summary>
        /// Check if a user can access a specific reading list
        /// </summary>
        /// <param name="listId">List ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if user can access the list</returns>
        Task<bool> CanUserAccessListAsync(Guid listId, Guid userId);

        /// <summary>
        /// Get the full hierarchy path for a reading list
        /// </summary>
        /// <param name="listId">List ID</param>
        /// <returns>List of lists from root to the specified list</returns>
        Task<List<ReadingList>> GetHierarchyPathAsync(Guid listId);
    }
}
