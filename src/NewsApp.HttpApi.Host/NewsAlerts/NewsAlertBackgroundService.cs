using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsApp.Domain.NewsAlerts;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using Volo.Abp.Uow;

namespace NewsApp.NewsAlerts.BackgroundWorkers
{
    public class NewsAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NewsAlertBackgroundService> _logger;
        private const int CheckIntervalMinutes = 2; // Cambia a 10 en producción
        private const int MaxArticlesPerCategory = 20;

        public NewsAlertBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NewsAlertBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News Alert Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting news alert check at {Time}", DateTime.UtcNow);
                    
                    await CheckAlertsAsync();
                    
                    _logger.LogInformation("Completed news alert check at {Time}", DateTime.UtcNow);
                    
                    _logger.LogInformation("Next check in {Minutes} minutes", CheckIntervalMinutes);
                    await Task.Delay(TimeSpan.FromMinutes(CheckIntervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("News Alert Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking news alerts");
                    // Wait before retrying on error
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("News Alert Background Service stopped");
        }

        private async Task CheckAlertsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            
            var alertListRepository = scope.ServiceProvider.GetRequiredService<INewsAlertListRepository>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INewsAlertNotificationRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<INewsAlertEmailService>();
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            
            // Create NewsAPI client
            var newsApiClient = new NewsApiClient("5ce39a327dab4cefa09559c6fe5d9de9");

            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            try
            {
                var activeAlerts = await alertListRepository.GetActiveAlertsAsync();
                
                _logger.LogInformation("Found {Count} active alerts to check", activeAlerts.Count);

                var notificationsByUser = new Dictionary<Guid, List<NewsAlertNotification>>();

                foreach (var alert in activeAlerts)
                {
                    try
                    {
                        _logger.LogInformation("Checking alert: {AlertName} for user {UserId}", alert.Name, alert.UserId);
                        
                        var categories = alert.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList();

                        foreach (var category in categories)
                        {
                            try
                            {
                                // Get news since last check or last 24 hours if never checked
                                var publishedAfter = alert.LastCheckedAt ?? DateTime.UtcNow.AddHours(-24);
                                
                                // Map category to NewsAPI category
                                var newsCategory = MapToNewsAPICategory(category);
                                
                                // Map language code
                                var language = MapToNewsAPILanguage(alert.LanguageCode);
                                
                                // Build query with OR for multiple keywords
                                var query = BuildKeywordQuery(alert.Keyword);
                                
                                // Call NewsAPI using /everything endpoint with q parameter for keyword search
                                var request = new EverythingRequest
                                {
                                    Q = query, // Search keyword in title, description and content
                                    Language = language,
                                    From = publishedAfter,
                                    PageSize = MaxArticlesPerCategory,
                                    SortBy = SortBys.PublishedAt
                                };
                                
                                var response = await newsApiClient.GetEverythingAsync(request);

                                if (response.Status == Statuses.Ok && response.Articles != null)
                                {
                                    // Articles are already filtered by keyword in the API
                                    var newArticles = response.Articles
                                        .Where(a => a.PublishedAt.HasValue && a.PublishedAt.Value > publishedAfter)
                                        .ToList();

                                    if (newArticles.Any())
                                    {
                                        var notification = new NewsAlertNotification(
                                            Guid.NewGuid(),
                                            alert.UserId,
                                            alert.Id,
                                            alert.Name,
                                            category,
                                            alert.LanguageCode,
                                            newArticles.Count,
                                            newArticles.Max(a => a.PublishedAt!.Value)
                                        );

                                        await notificationRepository.InsertAsync(notification);

                                        // Group notifications by user for batch email sending
                                        if (!notificationsByUser.ContainsKey(alert.UserId))
                                        {
                                            notificationsByUser[alert.UserId] = new List<NewsAlertNotification>();
                                        }
                                        notificationsByUser[alert.UserId].Add(notification);

                                        alert.MarkNewsFound();
                                        
                                        _logger.LogInformation(
                                            "Found {Count} new articles for alert '{AlertName}', category '{Category}'",
                                            newArticles.Count, alert.Name, category);
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("NewsAPI returned status: {Status}", response.Status);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error checking category {Category} for alert {AlertId}", category, alert.Id);
                            }
                        }

                        alert.MarkAsChecked();
                        await alertListRepository.UpdateAsync(alert);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing alert {AlertId}", alert.Id);
                    }
                }

                await uow.CompleteAsync();

                // Send emails after transaction is committed
                foreach (var (userId, notifications) in notificationsByUser)
                {
                    try
                    {
                        await emailService.SendNewAlertsEmailAsync(userId, notifications);
                        
                        _logger.LogInformation(
                            "Sent email to user {UserId} with {Count} notifications",
                            userId, notifications.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending email to user {UserId}", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAlertsAsync");
                throw;
            }
        }

        private Categories MapToNewsAPICategory(string category)
        {
            return category.ToLower() switch
            {
                "business" => Categories.Business,
                "entertainment" => Categories.Entertainment,
                "general" => Categories.Business, // NewsAPI doesn't have General, use Business as default
                "health" => Categories.Health,
                "science" => Categories.Science,
                "sports" => Categories.Sports,
                "technology" => Categories.Technology,
                _ => Categories.Business // Default to Business
            };
        }

        private Languages MapToNewsAPILanguage(string languageCode)
        {
            return languageCode.ToLower() switch
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

        private string BuildKeywordQuery(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return "news";
            }

            // Split by comma and trim whitespace
            var keywords = keyword.Split(',')
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .ToList();

            if (keywords.Count == 0)
            {
                return "news";
            }

            if (keywords.Count == 1)
            {
                return keywords[0];
            }

            // Build query with OR: "keyword1 OR keyword2 OR keyword3"
            return string.Join(" OR ", keywords);
        }
    }
}
