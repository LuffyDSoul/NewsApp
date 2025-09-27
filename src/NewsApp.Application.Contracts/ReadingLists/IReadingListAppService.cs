using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewsApp.ReadingLists
{
    public interface IReadingListAppService : IApplicationService
    {
        /// <summary>
        /// Gets all reading lists for the current user
        /// </summary>
        Task<List<ReadingListDto>> GetMyReadingListsAsync();

        /// <summary>
        /// Gets a specific reading list with articles
        /// </summary>
        Task<ReadingListWithArticlesDto> GetReadingListWithArticlesAsync(Guid id);

        /// <summary>
        /// Gets public reading lists
        /// </summary>
        Task<List<ReadingListDto>> GetPublicReadingListsAsync(int maxCount = 50);

        /// <summary>
        /// Creates a new reading list
        /// </summary>
        Task<ReadingListDto> CreateReadingListAsync(CreateReadingListDto input);

        /// <summary>
        /// Updates an existing reading list
        /// </summary>
        Task<ReadingListDto> UpdateReadingListAsync(Guid id, UpdateReadingListDto input);

        /// <summary>
        /// Deletes a reading list
        /// </summary>
        Task DeleteReadingListAsync(Guid id);

        /// <summary>
        /// Reorders reading lists
        /// </summary>
        Task ReorderReadingListsAsync(Dictionary<Guid, int> listOrders);
    }
}