using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NewsApp.Domain.NewsAlerts;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.NewsAlerts
{
    [Authorize]
    public class NewsAlertAppService : ApplicationService, INewsAlertAppService
    {
        private readonly INewsAlertListRepository _alertListRepository;
        private readonly INewsAlertNotificationRepository _notificationRepository;

        public NewsAlertAppService(
            INewsAlertListRepository alertListRepository,
            INewsAlertNotificationRepository notificationRepository)
        {
            _alertListRepository = alertListRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<List<NewsAlertListDto>> GetMyAlertsAsync(bool? isActive = null)
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            var alerts = await _alertListRepository.GetUserAlertsAsync(userId, isActive);
            
            return ObjectMapper.Map<List<NewsAlertList>, List<NewsAlertListDto>>(alerts);
        }

        public async Task<NewsAlertListDto> GetAsync(Guid id)
        {
            var alert = await _alertListRepository.GetAsync(id);
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Ensure user can only access their own alerts
            if (alert.UserId != userId)
            {
                throw new BusinessException("You can only access your own alerts");
            }
            
            return ObjectMapper.Map<NewsAlertList, NewsAlertListDto>(alert);
        }

        public async Task<NewsAlertListDto> CreateAsync(CreateNewsAlertListDto input)
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Check if alert with same name already exists for this user
            var existingAlert = await _alertListRepository.GetByNameAsync(userId, input.Name);
            if (existingAlert != null)
            {
                throw new BusinessException("An alert with this name already exists");
            }
            
            // Validate categories (should be comma-separated)
            var categories = input.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (categories.Length == 0)
            {
                throw new BusinessException("At least one category is required");
            }
            
            var alert = new NewsAlertList(
                GuidGenerator.Create(),
                userId,
                input.Name,
                input.Categories.Trim(),
                input.LanguageCode.ToLower(),
                input.Description?.Trim(),
                input.Keyword?.Trim(),
                input.IsActive
            );
            
            await _alertListRepository.InsertAsync(alert);
            
            return ObjectMapper.Map<NewsAlertList, NewsAlertListDto>(alert);
        }

        public async Task<NewsAlertListDto> UpdateAsync(Guid id, UpdateNewsAlertListDto input)
        {
            var alert = await _alertListRepository.GetAsync(id);
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Ensure user can only update their own alerts
            if (alert.UserId != userId)
            {
                throw new BusinessException("You can only update your own alerts");
            }
            
            // Check if another alert with same name exists
            var existingAlert = await _alertListRepository.GetByNameAsync(alert.UserId, input.Name);
            if (existingAlert != null && existingAlert.Id != id)
            {
                throw new BusinessException("An alert with this name already exists");
            }
            
            // Validate categories
            var categories = input.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (categories.Length == 0)
            {
                throw new BusinessException("At least one category is required");
            }
            
            alert.Update(
                input.Name,
                input.Categories.Trim(),
                input.LanguageCode.ToLower(),
                input.Description?.Trim(),
                input.Keyword?.Trim(),
                input.IsActive
            );
            
            await _alertListRepository.UpdateAsync(alert);
            
            return ObjectMapper.Map<NewsAlertList, NewsAlertListDto>(alert);
        }

        public async Task DeleteAsync(Guid id)
        {
            var alert = await _alertListRepository.GetAsync(id);
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Ensure user can only delete their own alerts
            if (alert.UserId != userId)
            {
                throw new BusinessException("You can only delete your own alerts");
            }
            
            await _alertListRepository.DeleteAsync(alert);
        }

        public async Task<List<NewsAlertNotificationDto>> GetMyNotificationsAsync(bool? unreadOnly = null, int maxCount = 50)
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId, 
                unreadOnly, 
                maxDaysOld: 7, 
                maxCount: maxCount
            );
            
            return ObjectMapper.Map<List<NewsAlertNotification>, List<NewsAlertNotificationDto>>(notifications);
        }

        public async Task<int> GetUnreadNotificationsCountAsync()
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(Guid id)
        {
            var notification = await _notificationRepository.GetAsync(id);
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Ensure user can only mark their own notifications as read
            if (notification.UserId != userId)
            {
                throw new BusinessException("You can only mark your own notifications as read");
            }
            
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task MarkAllNotificationsAsReadAsync()
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }
    }
}
