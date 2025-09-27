using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Guids;
using NewsApp.Domain.Alerts;
using NewsApp.Domain.Alerts.Repositories;
using NewsApp.Domain.Alerts.Services;
using NewsApp.Domain.News.Services;
using NewsApp.News;
using NewsApp.Permissions;
using NewsApp.Application; // Add this using directive

namespace NewsApp.Alerts
{
    /// <summary>
    /// Extension methods for Alert entity
    /// </summary>
    public static class AlertExtensions
    {
        /// <summary>
        /// Converts Alert entity to DTO
        /// </summary>
        public static AlertDto ToDto(this Alert alert)
        {
            return new AlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                AlertType = alert.AlertType,
                QueryText = alert.QueryText,
                ListId = alert.ListId,
                Active = alert.Active,
                Frequency = alert.Frequency,
                LastRun = alert.LastRun,
                LastNotificationSent = alert.LastNotificationSent,
                Name = alert.Name,
                LanguageCode = alert.LanguageCode,
                EmailNotifications = alert.EmailNotifications,
                MinimumResultsToNotify = alert.MinimumResultsToNotify,
                CreationTime = alert.CreationTime,
                NextRunTime = alert.NextRunTime,
                Keywords = alert.Keywords,
                Description = alert.Description
            };
        }

        /// <summary>
        /// Converts Alert entity to AlertWithResultsDto
        /// </summary>
        public static AlertWithResultsDto ToWithResultsDto(this Alert alert)
        {
            return new AlertWithResultsDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                AlertType = alert.AlertType,
                QueryText = alert.QueryText,
                ListId = alert.ListId,
                Active = alert.Active,
                Frequency = alert.Frequency,
                LastRun = alert.LastRun,
                LastNotificationSent = alert.LastNotificationSent,
                Name = alert.Name,
                LanguageCode = alert.LanguageCode,
                EmailNotifications = alert.EmailNotifications,
                MinimumResultsToNotify = alert.MinimumResultsToNotify,
                CreationTime = alert.CreationTime,
                NextRunTime = alert.NextRunTime,
                Keywords = alert.Keywords,
                Description = alert.Description,
                RecentResults = Array.Empty<AlertResultDto>()
            };
        }
    }

    /// <summary>
    /// Extension methods for AlertResult entity
    /// </summary>
    public static class AlertResultExtensions
    {
        /// <summary>
        /// Converts AlertResult entity to DTO
        /// </summary>
        public static AlertResultDto ToDto(this AlertResult result)
        {
            return new AlertResultDto
            {
                Id = result.Id,
                AlertId = result.AlertId,
                RunAt = result.CreationTime,
                FoundCount = result.ArticlesFound,
                Success = result.Status == AlertExecutionStatus.Success,
                ErrorMessage = result.ErrorMessage,
                NotificationSent = false, // Would need additional logic
                CreationTime = result.CreationTime,
                Status = result.Status,
                ArticlesFound = result.ArticlesFound
            };
        }
    }

    /// <summary>
    /// Application service for alert management
    /// </summary>
    [Authorize]
    public class AlertAppService : NewsAppAppService, IAlertAppService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IAlertResultRepository _alertResultRepository;
        private readonly INewsProvider _newsProvider;
        private readonly IAlertScheduler _alertScheduler;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;

        public AlertAppService(
            IAlertRepository alertRepository,
            IAlertResultRepository alertResultRepository,
            INewsProvider newsProvider,
            IAlertScheduler alertScheduler,
            IBackgroundJobManager backgroundJobManager,
            ICurrentUser currentUser,
            IGuidGenerator guidGenerator)
        {
            _alertRepository = alertRepository;
            _alertResultRepository = alertResultRepository;
            _newsProvider = newsProvider;
            _alertScheduler = alertScheduler;
            _backgroundJobManager = backgroundJobManager;
            _currentUser = currentUser;
            _guidGenerator = guidGenerator;
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<List<AlertDto>> GetMyAlertsAsync(bool includeInactive = false)
        {
            var userId = _currentUser.GetId();
            var alerts = await _alertRepository.GetByUserIdAsync(userId);
            
            if (!includeInactive)
            {
                alerts = alerts.Where(a => a.IsActive).ToList();
            }

            return alerts.ToDto(a => a.ToDto());
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<AlertWithResultsDto> GetWithResultsAsync(Guid id, int resultCount = 10)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this alert");

            var results = await _alertResultRepository.GetByAlertIdAsync(id, resultCount);
            
            var dto = alert.ToWithResultsDto();
            dto.RecentResults = results.Select(r => r.ToDto()).ToArray();
            
            return dto;
        }

        [Authorize(NewsAppPermissions.Alerts.Create)]
        public async Task<AlertDto> CreateAsync(CreateAlertDto input)
        {
            var userId = _currentUser.GetId();

            var alert = new Alert(
                GuidGenerator.Create(),
                input.Name,
                input.Keywords,
                userId,
                input.Description);

            // Configure frequency
            alert.SetFrequency(input.Frequency, input.FrequencyUnit);

            // Set optional filters
            if (input.SourceFilters?.Any() == true)
                alert.SetSourceFilters(input.SourceFilters);

            if (input.CategoryFilters?.Any() == true)
                alert.SetCategoryFilters(input.CategoryFilters);

            if (input.ExcludeKeywords?.Any() == true)
                alert.SetExcludeKeywords(input.ExcludeKeywords);

            if (!string.IsNullOrEmpty(input.Language))
                alert.SetLanguage(input.Language);

            if (input.Country?.Any() == true)
                alert.SetCountryFilter(input.Country);

            if (input.DateFrom.HasValue)
                alert.SetDateRange(input.DateFrom.Value, input.DateTo);

            // Set notification preferences
            if (input.EmailNotificationEnabled)
                alert.EnableEmailNotifications(input.NotificationEmail);

            if (input.PushNotificationEnabled)
                alert.EnablePushNotifications();

            if (input.InAppNotificationEnabled)
                alert.EnableInAppNotifications();

            // Set auto-activation
            if (input.ActivateImmediately)
                alert.Activate();

            var createdAlert = await _alertRepository.InsertAsync(alert, autoSave: true);

            // Schedule the alert if it's active
            if (alert.IsActive)
                await ScheduleAlertAsync(alert);

            return createdAlert.ToDto();
        }

        [Authorize(NewsAppPermissions.Alerts.Edit)]
        public async Task<AlertDto> UpdateAsync(Guid id, UpdateAlertDto input)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot update an alert you don't own");

            var wasActive = alert.IsActive;

            // Update basic info
            alert.SetBasicInfo(input.Name, input.Keywords, input.Description);

            // Update frequency
            alert.SetFrequency(input.Frequency, input.FrequencyUnit);

            // Update filters
            alert.SetSourceFilters(input.SourceFilters ?? new List<string>());
            alert.SetCategoryFilters(input.CategoryFilters ?? new List<string>());
            alert.SetExcludeKeywords(input.ExcludeKeywords ?? new List<string>());

            if (!string.IsNullOrEmpty(input.Language))
                alert.SetLanguage(input.Language);
            else
                alert.ClearLanguage();

            alert.SetCountryFilter(input.Country ?? new List<string>());

            if (input.DateFrom.HasValue)
                alert.SetDateRange(input.DateFrom.Value, input.DateTo);
            else
                alert.ClearDateRange();

            // Update notification preferences
            if (input.EmailNotificationEnabled)
                alert.EnableEmailNotifications(input.NotificationEmail);
            else
                alert.DisableEmailNotifications();

            if (input.PushNotificationEnabled)
                alert.EnablePushNotifications();
            else
                alert.DisablePushNotifications();

            if (input.InAppNotificationEnabled)
                alert.EnableInAppNotifications();
            else
                alert.DisableInAppNotifications();

            var updatedAlert = await _alertRepository.UpdateAsync(alert, autoSave: true);

            // Reschedule if active
            if (alert.IsActive)
                await ScheduleAlertAsync(alert);
            else if (wasActive)
                await UnscheduleAlertAsync(alert);

            return updatedAlert.ToDto();
        }

        [Authorize(NewsAppPermissions.Alerts.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot delete an alert you don't own");

            // Unschedule if active
            if (alert.IsActive)
                await UnscheduleAlertAsync(alert);

            await _alertRepository.DeleteAsync(id);
        }

        [Authorize(NewsAppPermissions.Alerts.Manage)]
        public async Task ActivateAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot activate an alert you don't own");

            alert.Activate();
            await _alertRepository.UpdateAsync(alert, autoSave: true);
            await ScheduleAlertAsync(alert);
        }

        [Authorize(NewsAppPermissions.Alerts.Manage)]
        public async Task DeactivateAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot deactivate an alert you don't own");

            alert.Deactivate();
            await _alertRepository.UpdateAsync(alert, autoSave: true);
            await UnscheduleAlertAsync(alert);
        }

        [Authorize(NewsAppPermissions.Alerts.Execute)]
        public async Task<AlertExecutionResultDto> ExecuteNowAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot execute an alert you don't own");

            return await ExecuteAlertInternalAsync(alert);
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<List<AlertResultDto>> GetAlertResultsAsync(Guid alertId, int count = 20)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(alertId);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this alert");

            var results = await _alertResultRepository.GetByAlertIdAsync(alertId, count);
            return results.ToDto(r => r.ToDto());
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<List<AlertDto>> GetActiveAlertsAsync()
        {
            var userId = _currentUser.GetId();
            var alerts = await _alertRepository.GetActiveByUserIdAsync(userId);
            return alerts.ToDto(a => a.ToDto());
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<List<AlertDto>> GetAlertsByKeywordAsync(string keyword)
        {
            var userId = _currentUser.GetId();
            var alerts = await _alertRepository.GetByKeywordAsync(userId, keyword);
            return alerts.ToDto(a => a.ToDto());
        }

        [Authorize(NewsAppPermissions.Alerts.Default)]
        public async Task<AlertStatisticsDto> GetAlertStatisticsAsync(Guid alertId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(alertId);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this alert");

            var fromDateTime = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var toDateTime = toDate ?? DateTime.UtcNow;

            var results = await _alertResultRepository.GetByAlertIdInPeriodAsync(alertId, fromDateTime, toDateTime);
            
            return new AlertStatisticsDto
            {
                AlertId = alertId,
                AlertName = alert.Name,
                PeriodStart = fromDateTime,
                PeriodEnd = toDateTime,
                TotalExecutions = results.Count,
                SuccessfulExecutions = results.Count(r => r.Status == AlertExecutionStatus.Success),
                FailedExecutions = results.Count(r => r.Status == AlertExecutionStatus.Failed),
                TotalArticlesFound = results.Where(r => r.Status == AlertExecutionStatus.Success).Sum(r => r.ArticlesFound),
                AverageArticlesPerExecution = results.Any(r => r.Status == AlertExecutionStatus.Success) 
                    ? results.Where(r => r.Status == AlertExecutionStatus.Success).Average(r => r.ArticlesFound) 
                    : 0,
                LastExecution = results.OrderByDescending(r => r.CreationTime).FirstOrDefault()?.CreationTime,
                NextScheduledExecution = alert.NextRunTime
            };
        }

        [Authorize(NewsAppPermissions.Alerts.Manage)]
        public async Task<bool> TestAlertAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot test an alert you don't own");

            try
            {
                var result = await ExecuteAlertInternalAsync(alert);
                return result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        [Authorize(NewsAppPermissions.Alerts.Import)]
        public async Task<List<AlertDto>> ImportAlertsAsync(ImportAlertsDto input)
        {
            var userId = _currentUser.GetId();
            var importedAlerts = new List<Alert>();

            foreach (var alertData in input.Alerts)
            {
                try
                {
                    var alert = new Alert(
                        GuidGenerator.Create(),
                        alertData.Name,
                        alertData.Keywords,
                        userId,
                        alertData.Description);

                    alert.SetFrequency(alertData.Frequency, alertData.FrequencyUnit);
                    
                    if (alertData.SourceFilters?.Any() == true)
                        alert.SetSourceFilters(alertData.SourceFilters);

                    if (alertData.CategoryFilters?.Any() == true)
                        alert.SetCategoryFilters(alertData.CategoryFilters);

                    if (!string.IsNullOrEmpty(alertData.Language))
                        alert.SetLanguage(alertData.Language);

                    if (input.ActivateImported)
                        alert.Activate();

                    var createdAlert = await _alertRepository.InsertAsync(alert, autoSave: false);
                    importedAlerts.Add(createdAlert);

                    if (alert.IsActive)
                        await ScheduleAlertAsync(alert);
                }
                catch
                {
                    // Skip invalid alerts
                }
            }

            await _alertRepository.SaveChangesAsync();
            return importedAlerts.ToDto(a => a.ToDto());
        }

        [Authorize(NewsAppPermissions.Alerts.Export)]
        public async Task<ExportAlertsResultDto> ExportAlertsAsync(ExportAlertsDto input)
        {
            var userId = _currentUser.GetId();
            List<Alert> alerts;

            if (input.AlertIds?.Any() == true)
            {
                alerts = await _alertRepository.GetByIdsAsync(input.AlertIds);
                // Filter to user's alerts only
                alerts = alerts.Where(a => a.UserId == userId).ToList();
            }
            else
            {
                alerts = await _alertRepository.GetByUserIdAsync(userId);
            }

            var exportData = alerts.Select(alert => new ExportAlertDto
            {
                Name = alert.Name,
                Keywords = alert.Keywords,
                Description = alert.Description,
                Frequency = alert.Frequency,
                FrequencyUnit = alert.FrequencyUnit,
                SourceFilters = alert.SourceFilters,
                CategoryFilters = alert.CategoryFilters,
                ExcludeKeywords = alert.ExcludeKeywords,
                Language = alert.Language,
                Country = alert.Country,
                IsActive = alert.IsActive,
                EmailNotificationEnabled = alert.EmailNotificationEnabled,
                PushNotificationEnabled = alert.PushNotificationEnabled,
                InAppNotificationEnabled = alert.InAppNotificationEnabled
            }).ToList();

            var exportContent = input.Format?.ToLower() switch
            {
                "json" => System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
                "csv" => ExportAlertsToCsv(exportData),
                _ => System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
            };

            return new ExportAlertsResultDto
            {
                Content = exportContent,
                FileName = $"alerts_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{input.Format ?? "json"}",
                ContentType = input.Format?.ToLower() switch
                {
                    "csv" => "text/csv",
                    _ => "application/json"
                },
                AlertCount = alerts.Count
            };
        }

        [Authorize(NewsAppPermissions.Alerts.Manage)]
        public async Task SnoozeAlertAsync(Guid id, int snoozeMinutes)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot snooze an alert you don't own");

            if (snoozeMinutes <= 0 || snoozeMinutes > 1440) // Max 24 hours
                throw new ArgumentException("Snooze time must be between 1 and 1440 minutes");

            alert.SetNextRunTime(DateTime.UtcNow.AddMinutes(snoozeMinutes));
            await _alertRepository.UpdateAsync(alert, autoSave: true);
            await ScheduleAlertAsync(alert);
        }

        [Authorize(NewsAppPermissions.Alerts.Manage)]
        public async Task ResetAlertScheduleAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var alert = await _alertRepository.GetAsync(id);
            
            if (alert.UserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot reset schedule for an alert you don't own");

            alert.ResetNextRunTime();
            await _alertRepository.UpdateAsync(alert, autoSave: true);
            
            if (alert.IsActive)
                await ScheduleAlertAsync(alert);
        }

        private async Task<AlertExecutionResultDto> ExecuteAlertInternalAsync(Alert alert)
        {
            var executionId = GuidGenerator.Create();
            var startTime = DateTime.UtcNow;

            try
            {
                // Build search criteria
                var searchCriteria = new NewsSearchCriteria
                {
                    Query = alert.Keywords,
                    Sources = alert.SourceFilters,
                    Categories = alert.CategoryFilters,
                    Language = alert.Language,
                    Country = alert.Country?.FirstOrDefault(),
                    ExcludeKeywords = alert.ExcludeKeywords,
                    From = alert.DateFrom,
                    To = alert.DateTo,
                    PageSize = 100 // Default search limit
                };

                // Execute search
                var searchResult = await _newsProvider.SearchAsync(searchCriteria);
                
                // Filter articles based on exclude keywords
                var filteredArticles = FilterArticlesByExcludeKeywords(searchResult.Articles, alert.ExcludeKeywords);

                // Record execution result
                var alertResult = new AlertResult(
                    executionId,
                    alert.Id,
                    AlertExecutionStatus.Success,
                    filteredArticles.Count,
                    DateTime.UtcNow - startTime,
                    null);

                await _alertResultRepository.InsertAsync(alertResult, autoSave: true);

                // Update alert's last run time and calculate next run
                alert.RecordExecution(DateTime.UtcNow);
                await _alertRepository.UpdateAsync(alert, autoSave: true);

                // Send notifications if articles found
                if (filteredArticles.Any() && alert.HasNotificationsEnabled())
                {
                    await SendAlertNotificationsAsync(alert, filteredArticles);
                }

                return new AlertExecutionResultDto
                {
                    ExecutionId = executionId,
                    Success = true,
                    ArticlesFound = filteredArticles.Count,
                    Duration = DateTime.UtcNow - startTime,
                    Message = $"Found {filteredArticles.Count} articles matching your criteria"
                };
            }
            catch (Exception ex)
            {
                // Record failed execution
                var alertResult = new AlertResult(
                    executionId,
                    alert.Id,
                    AlertExecutionStatus.Failed,
                    0,
                    DateTime.UtcNow - startTime,
                    ex.Message);

                await _alertResultRepository.InsertAsync(alertResult, autoSave: true);

                return new AlertExecutionResultDto
                {
                    ExecutionId = executionId,
                    Success = false,
                    ArticlesFound = 0,
                    Duration = DateTime.UtcNow - startTime,
                    Message = $"Execution failed: {ex.Message}"
                };
            }
        }

        private async Task ScheduleAlertAsync(Alert alert)
        {
            if (alert.NextRunTime.HasValue)
            {
                // Schedule background job for alert execution
                await _backgroundJobManager.EnqueueAsync(
                    new ProcessAlertJob { AlertId = alert.Id },
                    delay: alert.NextRunTime.Value - DateTime.UtcNow);
            }
        }

        private async Task UnscheduleAlertAsync(Alert alert)
        {
            // In a real implementation, you would cancel scheduled jobs
            // This is simplified for demo purposes
            await Task.CompletedTask;
        }

        private async Task SendAlertNotificationsAsync(Alert alert, List<Domain.News.NewsArticle> articles)
        {
            // This would integrate with your notification system
            // For now, just a placeholder
            await Task.CompletedTask;
        }

        private static List<Domain.News.NewsArticle> FilterArticlesByExcludeKeywords(
            List<Domain.News.NewsArticle> articles, 
            List<string> excludeKeywords)
        {
            if (!excludeKeywords?.Any() == true)
                return articles;

            return articles.Where(article =>
            {
                var content = $"{article.Title} {article.Description}".ToLower();
                return !excludeKeywords.Any(keyword => content.Contains(keyword.ToLower()));
            }).ToList();
        }

        private static string ExportAlertsToCsv(List<ExportAlertDto> alerts)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Name,Keywords,Description,Frequency,FrequencyUnit,IsActive,EmailEnabled,PushEnabled,InAppEnabled");
            
            foreach (var alert in alerts)
            {
                csv.AppendLine($"\"{alert.Name}\",\"{alert.Keywords}\",\"{alert.Description}\",{alert.Frequency},{alert.FrequencyUnit},{alert.IsActive},{alert.EmailNotificationEnabled},{alert.PushNotificationEnabled},{alert.InAppNotificationEnabled}");
            }
            
            return csv.ToString();
        }

        // Background job class for processing alerts
        public class ProcessAlertJob
        {
            public Guid AlertId { get; set; }
        }

        #region Missing Interface Implementations

        public async Task<AlertDto> GetAsync(Guid id)
        {
            var alert = await _alertRepository.GetAsync(id);
            return ObjectMapper.Map<Alert, AlertDto>(alert);
        }

        public async Task<bool> TriggerAsync(Guid id)
        {
            try
            {
                await ExecuteNowAsync(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<PagedResultDto<AlertResultDto>> GetResultsAsync(Guid alertId, int skipCount = 0, int maxResultCount = 10)
        {
            // Stub implementation
            return Task.FromResult(new PagedResultDto<AlertResultDto>
            {
                TotalCount = 0,
                Items = new List<AlertResultDto>()
            });
        }

        public Task<RecentAlertActivityDto> GetRecentActivityAsync(int days = 7)
        {
            // Stub implementation
            return Task.FromResult(new RecentAlertActivityDto
            {
                RecentResults = Array.Empty<AlertResultDto>(),
                RecentlyTriggeredAlerts = Array.Empty<AlertDto>(),
                TotalNotificationsThisWeek = 0,
                LastActivityTime = DateTime.UtcNow
            });
        }

        public async Task<AlertStatisticsDto> GetStatisticsAsync(DateTime? since = null)
        {
            // Stub implementation
            return new AlertStatisticsDto
            {
                TotalAlerts = (int)await _alertRepository.GetCountAsync(),
                ActiveAlerts = (int)await _alertRepository.GetCountAsync(),
                TotalExecutions = 0,
                SuccessfulExecutions = 0,
                FailedExecutions = 0,
                NotificationsSent = 0,
                AverageExecutionTime = 0,
                LastExecution = null,
                SuccessRate = 0,
                FailureRate = 0
            };
        }

        public Task<List<AlertResultDto>> GetRecentNotificationsAsync()
        {
            // Stub implementation
            return Task.FromResult(new List<AlertResultDto>());
        }

        public Task<AlertTestResultDto> TestAlertAsync(CreateAlertDto input)
        {
            // Stub implementation
            return Task.FromResult(new AlertTestResultDto
            {
                Success = true,
                FoundCount = 0,
                ErrorMessage = null,
                SampleArticles = new List<NewsArticlePreviewDto>(),
                TestedAt = DateTime.UtcNow
            });
        }

        public async Task<List<AlertDto>> GetByTypeAsync(Domain.Alerts.AlertType alertType)
        {
            var alerts = await _alertRepository.GetListAsync(x => x.AlertType == alertType && x.UserId == CurrentUser.Id);
            return ObjectMapper.Map<List<Alert>, List<AlertDto>>(alerts);
        }

        public async Task<AlertDto> CloneAsync(Guid id, string newName)
        {
            var originalAlert = await _alertRepository.GetAsync(id);
            var clonedAlert = new Alert(
                _guidGenerator.Create(),
                newName,
                originalAlert.Keywords,
                originalAlert.UserId,
                originalAlert.Description
            );
            
            await _alertRepository.InsertAsync(clonedAlert);
            return ObjectMapper.Map<Alert, AlertDto>(clonedAlert);
        }

        public async Task UpdateNotificationPreferencesAsync(Guid id, bool emailNotifications, int minimumResultsToNotify)
        {
            var alert = await _alertRepository.GetAsync(id);
            alert.SetNotificationPreferences(emailNotifications, minimumResultsToNotify);
            await _alertRepository.UpdateAsync(alert);
        }

        public async Task<List<AlertDto>> GetByListIdAsync(Guid listId)
        {
            var alerts = await _alertRepository.GetListAsync(x => x.ListId == listId && x.UserId == CurrentUser.Id);
            return ObjectMapper.Map<List<Alert>, List<AlertDto>>(alerts);
        }

        public async Task BulkUpdateActiveStatusAsync(List<Guid> alertIds, bool active)
        {
            var alerts = await _alertRepository.GetListAsync(x => alertIds.Contains(x.Id) && x.UserId == CurrentUser.Id);
            foreach (var alert in alerts)
            {
                if (active)
                    alert.Activate();
                else
                    alert.Deactivate();
            }
            await _alertRepository.UpdateManyAsync(alerts);
        }

        #endregion
    }
}
