using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewsApp.NewsAlerts
{
    public interface INewsAlertAppService : IApplicationService
    {
        Task<List<NewsAlertListDto>> GetMyAlertsAsync(bool? isActive = null);
        
        Task<NewsAlertListDto> GetAsync(Guid id);
        
        Task<NewsAlertListDto> CreateAsync(CreateNewsAlertListDto input);
        
        Task<NewsAlertListDto> UpdateAsync(Guid id, UpdateNewsAlertListDto input);
        
        Task DeleteAsync(Guid id);
        
        Task<List<NewsAlertNotificationDto>> GetMyNotificationsAsync(bool? unreadOnly = null, int maxCount = 50);
        
        Task<int> GetUnreadNotificationsCountAsync();
        
        Task MarkNotificationAsReadAsync(Guid id);
        
        Task MarkAllNotificationsAsReadAsync();
        
        Task<bool> TestAlertAsync(Guid id);
    }
}

