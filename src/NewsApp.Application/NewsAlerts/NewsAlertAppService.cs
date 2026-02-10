using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsApp.Domain.NewsAlerts;
using NewsApp.News;
using NewsApp.Email;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace NewsApp.NewsAlerts
{
    [Authorize]
    public class NewsAlertAppService : ApplicationService, INewsAlertAppService
    {
        private readonly INewsAlertListRepository _alertListRepository;
        private readonly INewsAlertNotificationRepository _notificationRepository;
        private readonly INewsService _newsService;
        private readonly IEmailService _emailService;
        private readonly IIdentityUserRepository _userRepository;

        public NewsAlertAppService(
            INewsAlertListRepository alertListRepository,
            INewsAlertNotificationRepository notificationRepository,
            INewsService newsService,
            IEmailService emailService,
            IIdentityUserRepository userRepository)
        {
            _alertListRepository = alertListRepository;
            _notificationRepository = notificationRepository;
            _newsService = newsService;
            _emailService = emailService;
            _userRepository = userRepository;
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
            
            // Validate that keyword is provided
            if (string.IsNullOrWhiteSpace(input.Keyword))
            {
                throw new BusinessException("Keyword is required");
            }
            
            // Check if alert with same name already exists for this user
            try
            {
                var existingAlert = await _alertListRepository.GetByNameAsync(userId, input.Name);
                if (existingAlert != null && !existingAlert.IsDeleted)
                {
                    throw new BusinessException($"An alert with the name '{input.Name}' already exists. Please choose a different name.");
                }
            }
            catch (BusinessException)
            {
                throw; // Re-throw business exceptions
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error checking for existing alert with name {Name}", input.Name);
                // Continue with creation - if it truly exists, DB will catch it
            }
            
            // Use "general" as placeholder for categories since we're keyword-only now
            var categories = "general";
            
            var alert = new NewsAlertList(
                GuidGenerator.Create(),
                userId,
                input.Name,
                categories,
                input.LanguageCode.ToLower(),
                input.Description?.Trim(),
                input.Keyword?.Trim(),
                input.IsActive
            );
            
            try
            {
                await _alertListRepository.InsertAsync(alert);
                return ObjectMapper.Map<NewsAlertList, NewsAlertListDto>(alert);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_AppNewsAlertLists_UserId_Name") == true)
            {
                Logger.LogWarning(ex, "Duplicate alert name detected for user {UserId} with name {Name}", userId, input.Name);
                throw new BusinessException($"An alert with the name '{input.Name}' already exists. Please choose a different name or delete the existing one first.");
            }
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
            
            // Check if another active alert with same name exists (excluding soft-deleted ones)
            var existingAlert = await _alertListRepository.GetByNameAsync(alert.UserId, input.Name);
            if (existingAlert != null && existingAlert.Id != id && !existingAlert.IsDeleted)
            {
                throw new BusinessException("An alert with this name already exists");
            }
            
            // Validate that keyword is provided
            if (string.IsNullOrWhiteSpace(input.Keyword))
            {
                throw new BusinessException("Keyword is required");
            }
            
            // Use "general" as placeholder for categories since we're keyword-only now
            var categories = "general";
            
            alert.Update(
                input.Name,
                categories,
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
            
            // Convert unreadOnly to isRead parameter
            // unreadOnly = true means we want only unread notifications (isRead = false)
            // unreadOnly = false means we want all notifications (isRead = null)
            // unreadOnly = null means we want all notifications (isRead = null)
            bool? isRead = unreadOnly.HasValue ? (unreadOnly.Value ? false : (bool?)null) : null;
            
            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId, 
                isRead, 
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

        public async Task<bool> TestAlertAsync(Guid id)
        {
            var alert = await _alertListRepository.GetAsync(id);
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            // Ensure user can only test their own alerts
            if (alert.UserId != userId)
            {
                throw new BusinessException("You can only test your own alerts");
            }

            if (!alert.IsActive)
            {
                throw new BusinessException("Cannot test an inactive alert. Please activate it first.");
            }

            try
            {
                // Get user information
                var user = await _userRepository.GetAsync(userId);
                
                // Check if email is configured
                if (string.IsNullOrEmpty(user.Email))
                {
                    throw new BusinessException("No email address configured in your profile. Please add an email address to receive notifications.");
                }

                Logger.LogInformation("Testing alert {AlertId} for user {UserId}", id, userId);

                // Build search query from alert
                var searchQuery = alert.Keyword ?? alert.Categories;
                
                // Search for news from last 7 days (more results)
                var articles = await _newsService.GetNewsAsync(
                    searchQuery, 
                    language: alert.LanguageCode,
                    from: DateTime.UtcNow.AddDays(-7), // Changed to 7 days for more results
                    pageSize: 20
                );

                Logger.LogInformation("Found {Count} articles for alert {AlertId}", articles.Count, id);

                if (articles.Count == 0)
                {
                    throw new BusinessException($"No news articles found for '{searchQuery}' in the last 7 days. Try using different keywords or check back later.");
                }

                // Send email with found articles
                await _emailService.SendNewsNotificationAsync(
                    to: user.Email,
                    userName: user.UserName ?? user.Email,
                    themeName: $"Alert Test: {alert.Name}",
                    articles: articles.Take(10).ToList()
                );

                Logger.LogInformation("Test email sent successfully to {Email} for alert {AlertName}", user.Email, alert.Name);
                
                return true;
            }
            catch (BusinessException)
            {
                throw; // Re-throw business exceptions
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error testing alert {AlertId} for user {UserId}", id, userId);
                throw new BusinessException("Failed to test alert: " + ex.Message);
            }
        }
    }
}



