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
    public class NewsAlertNotificationRepository : EfCoreRepository<NewsAppDbContext, NewsAlertNotification, Guid>, INewsAlertNotificationRepository
    {
        public NewsAlertNotificationRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<NewsAlertNotification>> GetUserNotificationsAsync(
            Guid userId, 
            bool? isRead = null, 
            int? maxDaysOld = 7,
            int maxCount = 50)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(x => x.UserId == userId);
            
            if (isRead.HasValue)
            {
                query = query.Where(x => x.IsRead == isRead.Value);
            }
            
            if (maxDaysOld.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-maxDaysOld.Value);
                query = query.Where(x => x.CreationTime >= cutoffDate);
            }
            
            return await query
                .OrderByDescending(x => x.CreationTime)
                .Take(maxCount)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.UserId == userId && !x.IsRead)
                .CountAsync();
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE NewsAlertNotifications SET IsRead = 1 WHERE UserId = {0} AND IsRead = 0",
                userId);
        }

        public async Task<List<NewsAlertNotification>> GetNotificationsForAlertListAsync(
            Guid newsAlertListId, 
            DateTime? since = null)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.Where(x => x.NewsAlertListId == newsAlertListId);
            
            if (since.HasValue)
            {
                query = query.Where(x => x.CreationTime >= since.Value);
            }
            
            return await query
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }
    }
}
