using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.Domain.NewsAlerts
{
    public interface INewsAlertNotificationRepository : IRepository<NewsAlertNotification, Guid>
    {
        Task<List<NewsAlertNotification>> GetUserNotificationsAsync(
            Guid userId, 
            bool? isRead = null, 
            int? maxDaysOld = 7,
            int maxCount = 50);
        
        Task<int> GetUnreadCountAsync(Guid userId);
        
        Task MarkAllAsReadAsync(Guid userId);
        
        Task<List<NewsAlertNotification>> GetNotificationsForAlertListAsync(
            Guid newsAlertListId, 
            DateTime? since = null);
    }
}
