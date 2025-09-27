using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.ReadingLists
{
    public interface IReadingListRepository : IRepository<ReadingList, Guid>
    {
        Task<List<ReadingList>> GetUserReadingListsAsync(
            Guid userId,
            bool includeArticles = false,
            CancellationToken cancellationToken = default);

        Task<ReadingList?> GetUserReadingListByNameAsync(
            Guid userId,
            string name,
            bool includeArticles = false,
            CancellationToken cancellationToken = default);

        Task<List<ReadingList>> GetPublicReadingListsAsync(
            int maxCount = 50,
            CancellationToken cancellationToken = default);

        Task<bool> UserOwnsListAsync(
            Guid userId,
            Guid listId,
            CancellationToken cancellationToken = default);
    }
}