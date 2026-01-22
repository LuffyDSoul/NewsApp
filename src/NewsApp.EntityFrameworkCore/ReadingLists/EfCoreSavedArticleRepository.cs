using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsApp.EntityFrameworkCore;
using NewsApp.ReadingLists;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace NewsApp.EntityFrameworkCore.ReadingLists
{
    public class EfCoreSavedArticleRepository : EfCoreRepository<NewsAppDbContext, SavedArticle, Guid>, ISavedArticleRepository
    {
        public EfCoreSavedArticleRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<SavedArticle>> GetUserSavedArticlesAsync(
            Guid userId,
            Guid? readingListId = null,
            bool onlyUnread = false,
            int maxCount = 100,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var query = dbContext.SavedArticles
                .Where(sa => sa.UserId == userId);

            if (readingListId.HasValue)
            {
                query = query.Where(sa => sa.ReadingListId == readingListId.Value);
            }

            if (onlyUnread)
            {
                query = query.Where(sa => !sa.IsRead);
            }

            return await query
                .Include(sa => sa.ReadingList)
                .OrderByDescending(sa => sa.SavedAt)
                .Take(maxCount)
                .ToListAsync(cancellationToken);
        }

        public async Task<SavedArticle?> GetUserSavedArticleByUrlAsync(
            Guid userId,
            string url,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.SavedArticles
                .Include(sa => sa.ReadingList)
                .FirstOrDefaultAsync(sa => sa.UserId == userId && sa.Url == url, cancellationToken);
        }

        public async Task<bool> IsArticleSavedByUserAsync(
            Guid userId,
            string url,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.SavedArticles
                .AnyAsync(sa => sa.UserId == userId && sa.Url == url, cancellationToken);
        }

        public async Task<List<SavedArticle>> GetArticlesByReadingListAsync(
            Guid readingListId,
            bool onlyUnread = false,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var query = dbContext.SavedArticles
                .Where(sa => sa.ReadingListId == readingListId);

            if (onlyUnread)
            {
                query = query.Where(sa => !sa.IsRead);
            }

            return await query
                .OrderByDescending(sa => sa.SavedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUserSavedArticlesCountAsync(
            Guid userId,
            bool onlyUnread = false,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var query = dbContext.SavedArticles.Where(sa => sa.UserId == userId);

            if (onlyUnread)
            {
                query = query.Where(sa => !sa.IsRead);
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}