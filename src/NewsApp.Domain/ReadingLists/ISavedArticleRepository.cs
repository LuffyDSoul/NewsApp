using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.ReadingLists
{
    public interface ISavedArticleRepository : IRepository<SavedArticle, Guid>
    {
        Task<List<SavedArticle>> GetUserSavedArticlesAsync(
            Guid userId,
            Guid? readingListId = null,
            bool onlyUnread = false,
            int maxCount = 100,
            CancellationToken cancellationToken = default);

        Task<SavedArticle?> GetUserSavedArticleByUrlAsync(
            Guid userId,
            string url,
            CancellationToken cancellationToken = default);

        Task<bool> IsArticleSavedByUserAsync(
            Guid userId,
            string url,
            CancellationToken cancellationToken = default);

        Task<List<SavedArticle>> GetArticlesByReadingListAsync(
            Guid readingListId,
            bool onlyUnread = false,
            CancellationToken cancellationToken = default);

        Task<int> GetUserSavedArticlesCountAsync(
            Guid userId,
            bool onlyUnread = false,
            CancellationToken cancellationToken = default);
    }
}