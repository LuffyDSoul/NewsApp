using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NewsApp.Domain.NewsAlerts;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
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
            
            // Validate that keyword is provided
            if (string.IsNullOrWhiteSpace(input.Keyword))
            {
                throw new BusinessException("Keyword is required");
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

        public async Task<string> TriggerManualCheckAsync()
        {
            var userId = CurrentUser.Id ?? throw new BusinessException("User is not authenticated");
            
            var activeAlerts = await _alertListRepository.GetActiveAlertsAsync();
            var userAlerts = activeAlerts.Where(a => a.UserId == userId).ToList();
            
            if (!userAlerts.Any())
            {
                return $"❌ No active alerts found for user {userId}. Please create at least one active alert.";
            }

            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");
            var results = new List<string>();
            var notificationCount = 0;

            results.Add($"📋 Checking {userAlerts.Count} active alert(s)...\n");

            foreach (var alert in userAlerts)
            {
                try
                {
                    // Validate that alert has a keyword
                    if (string.IsNullOrWhiteSpace(alert.Keyword))
                    {
                        results.Add($"🔔 Alert: '{alert.Name}'");
                        results.Add($"   ⚠️ Skipping - No keyword specified\n");
                        continue;
                    }

                    // Use last checked date as starting point, or last 7 days if never checked
                    var publishedAfter = alert.LastCheckedAt ?? DateTime.UtcNow.AddDays(-7);

                    var alertInfo = $"🔔 Alert: '{alert.Name}'";
                    alertInfo += $"\n   - Keyword: {alert.Keyword}";
                    alertInfo += $"\n   - Last checked: {(alert.LastCheckedAt.HasValue ? alert.LastCheckedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Never")}";
                    alertInfo += $"\n   - Searching from: {publishedAfter:yyyy-MM-dd HH:mm:ss}";
                    results.Add(alertInfo);

                    var request = new EverythingRequest
                    {
                        Q = alert.Keyword,
                        Language = MapLanguage(alert.LanguageCode),
                        From = publishedAfter,
                        PageSize = 20,
                        SortBy = SortBys.PublishedAt
                    };
                    
                    var response = await newsApiClient.GetEverythingAsync(request);
                    
                    if (response.Status == Statuses.Ok && response.Articles != null)
                    {
                        results.Add($"   ✓ API Response: {response.Articles.Count} total articles returned");
                        
                        // Log some sample dates for debugging
                        if (response.Articles.Any())
                        {
                            var sampleArticles = response.Articles.Take(3).Where(a => a.PublishedAt.HasValue).ToList();
                            foreach (var sample in sampleArticles)
                            {
                                var title = sample.Title ?? "[No title]";
                                var shortTitle = title.Length > 50 ? title.Substring(0, 50) : title;
                                results.Add($"      Sample: {shortTitle}... published at {sample.PublishedAt:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                        
                        var newArticles = response.Articles
                            .Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value > publishedAfter)
                            .ToList();

                        results.Add($"   → {newArticles.Count} articles are newer than {publishedAfter:yyyy-MM-dd HH:mm:ss}");

                        if (newArticles.Any())
                        {
                            // Update LastCheckedAt to the most recent article date
                            var mostRecentArticleDate = newArticles.Max(a => a.PublishedAt!.Value);
                            alert.LastCheckedAt = mostRecentArticleDate;
                            
                            var articleUrls = string.Join(",", newArticles.Select(a => a.Url));
                            
                            var notification = new NewsAlertNotification(
                                GuidGenerator.Create(),
                                alert.UserId,
                                alert.Id,
                                alert.Name,
                                "keyword",
                                alert.LanguageCode,
                                newArticles.Count,
                                mostRecentArticleDate,
                                articleUrls
                            );

                            await _notificationRepository.InsertAsync(notification);
                            notificationCount++;
                            results.Add($"   ✅ Created notification! Found {newArticles.Count} new articles");
                            results.Add($"   ⏰ Updated LastCheckedAt to: {mostRecentArticleDate:yyyy-MM-dd HH:mm:ss}");
                            
                            // Update last news found time
                            alert.MarkNewsFound();
                        }
                        else
                        {
                            // No new articles, update LastCheckedAt to now so we don't keep checking the same period
                            alert.MarkAsChecked();
                            results.Add($"   ℹ️ No new articles found");
                            results.Add($"   ⏰ Updated LastCheckedAt to: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
                    else
                    {
                        results.Add($"   ❌ API Error: {response.Status}");
                    }

                    await _alertListRepository.UpdateAsync(alert);
                    results.Add(""); // Empty line between alerts
                }
                catch (Exception ex)
                {
                    results.Add($"   ❌ ERROR: {ex.Message}\n");
                }
            }

            if (CurrentUnitOfWork != null)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            var summary = $"{'=' * 50}\n";
            summary += $"📊 SUMMARY\n";
            summary += $"{'=' * 50}\n";
            summary += $"✓ Checked: {userAlerts.Count} alert(s)\n";
            summary += $"✓ Created: {notificationCount} notification(s)\n";
            summary += $"{'=' * 50}\n\n";

            return summary + string.Join("\n", results);
        }

        private Languages MapLanguage(string code)
        {
            return code.ToLower() switch
            {
                "de" => Languages.DE,
                "en" => Languages.EN,
                "es" => Languages.ES,
                "fr" => Languages.FR,
                "he" => Languages.HE,
                "it" => Languages.IT,
                "nl" => Languages.NL,
                "no" => Languages.NO,
                "pt" => Languages.PT,
                "sv" => Languages.SV,
                _ => Languages.EN
            };
        }

        private Categories MapCategory(string category)
        {
            return category.ToLower() switch
            {
                "business" => Categories.Business,
                "entertainment" => Categories.Entertainment,
                "health" => Categories.Health,
                "science" => Categories.Science,
                "sports" => Categories.Sports,
                "technology" => Categories.Technology,
                "general" => Categories.Business,
                _ => Categories.Business
            };
        }
    }
}
