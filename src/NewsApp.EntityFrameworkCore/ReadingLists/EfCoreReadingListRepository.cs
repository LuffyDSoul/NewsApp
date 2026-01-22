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
    public class EfCoreReadingListRepository : EfCoreRepository<NewsAppDbContext, ReadingList, Guid>, IReadingListRepository
    {
        public EfCoreReadingListRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<ReadingList>> GetUserReadingListsAsync(
            Guid userId,
            bool includeArticles = false,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var query = dbContext.ReadingLists
                .Where(rl => rl.UserId == userId);

            if (includeArticles)
            {
                query = query.Include(rl => rl.SavedArticles);
            }

            return await query
                .OrderBy(rl => rl.SortOrder)
                .ThenBy(rl => rl.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<ReadingList?> GetUserReadingListByNameAsync(
            Guid userId,
            string name,
            bool includeArticles = false,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var query = dbContext.ReadingLists
                .Where(rl => rl.UserId == userId && rl.Name == name);

            if (includeArticles)
            {
                query = query.Include(rl => rl.SavedArticles);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ReadingList>> GetPublicReadingListsAsync(
            int maxCount = 50,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ReadingLists
                .Where(rl => rl.IsPublic)
                .Include(rl => rl.User)
                .OrderByDescending(rl => rl.CreatedAt)
                .Take(maxCount)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UserOwnsListAsync(
            Guid userId,
            Guid listId,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            return await dbContext.ReadingLists
                .AnyAsync(rl => rl.Id == listId && rl.UserId == userId, cancellationToken);
        }
    }
}