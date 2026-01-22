using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.Domain.NewsAlerts
{
    public interface INewsAlertListRepository : IRepository<NewsAlertList, Guid>
    {
        Task<List<NewsAlertList>> GetActiveAlertsAsync();
        
        Task<List<NewsAlertList>> GetUserAlertsAsync(Guid userId, bool? isActive = null);
        
        Task<NewsAlertList?> GetByNameAsync(Guid userId, string name);
    }
}
