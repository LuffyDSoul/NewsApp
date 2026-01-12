using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsApp.Domain.NewsAlerts;
using NewsApp.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace NewsApp.NewsAlerts
{
    public class NewsAlertListRepository : EfCoreRepository<NewsAppDbContext, NewsAlertList, Guid>, INewsAlertListRepository
    {
        public NewsAlertListRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<NewsAlertList>> GetActiveAlertsAsync()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.IsActive && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<NewsAlertList>> GetUserAlertsAsync(Guid userId, bool? isActive = null)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(x => x.UserId == userId && !x.IsDeleted);
            
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }
            
            return await query
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }

        public async Task<NewsAlertList?> GetByNameAsync(Guid userId, string name)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name && !x.IsDeleted);
        }
    }
}
