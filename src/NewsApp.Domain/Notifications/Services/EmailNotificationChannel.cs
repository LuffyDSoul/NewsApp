using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.TextTemplating;
using NewsApp.Domain.Notifications.Services;

namespace NewsApp.Infrastructure.Notifications
{
    /// <summary>
    /// Email notification channel implementation using ABP's email service
    /// </summary>
    public class EmailNotificationChannel : INotificationChannel, ITransientDependency
    {
        private readonly IEmailSender _emailSender;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ILogger<EmailNotificationChannel> _logger;

        public string ChannelName => "Email";

        public EmailNotificationChannel(
            IEmailSender emailSender,
            ITemplateRenderer templateRenderer,
            IIdentityUserRepository userRepository,
            ILogger<EmailNotificationChannel> logger)
        {
            _emailSender = emailSender;
            _templateRenderer = templateRenderer;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<bool> SendAsync(NotificationMessage notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.Email))
                {
                    _logger.LogWarning("Cannot send email notification: email address is empty for user {UserId}", notification.UserId);
                    return false;
                }

                // Check user preferences
                var preferences = await GetUserPreferencesAsync(notification.UserId);
                if (!preferences.ShouldReceive(notification))
                {
                    _logger.LogDebug("Notification filtered out by user preferences for user {UserId}", notification.UserId);
                    return true; // Not an error, just filtered
                }

                // Determine if we should send now or schedule for later
                if (notification.ScheduledFor.HasValue && notification.ScheduledFor.Value > DateTime.UtcNow)
                {
                    // For now, we'll just wait - in a production system, you'd use a background job scheduler
                    _logger.LogInformation("Notification scheduled for {ScheduledTime} - implementing immediate send for now", notification.ScheduledFor.Value);
                }

                // Render template if needed
                var subject = notification.Subject;
                var body = notification.HtmlBody ?? notification.Body;

                if (notification.Type == "alert_triggered")
                {
                    body = await RenderAlertTemplate(notification);
                }
                else if (notification.Type == "reading_list_update")
                {
                    body = await RenderReadingListTemplate(notification);
                }

                // Send the email
                await _emailSender.SendAsync(
                    to: notification.Email,
                    subject: subject,
                    body: body,
                    isBodyHtml: !string.IsNullOrEmpty(notification.HtmlBody) || notification.Type.StartsWith("alert_") || notification.Type.StartsWith("reading_"));

                _logger.LogInformation("Email notification sent successfully to {Email} for user {UserId}", notification.Email, notification.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification to {Email} for user {UserId}", notification.Email, notification.UserId);
                return false;
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                // Check if email sender is properly configured
                // This is a simple check - in production you might want to send a test email
                return _emailSender != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email notification channel is not available");
                return false;
            }
        }

        public async Task<NotificationPreferences> GetUserPreferencesAsync(Guid userId)
        {
            try
            {
                // In a real implementation, you'd load these from user settings/preferences table
                // For now, return default preferences
                var preferences = new NotificationPreferences
                {
                    Enabled = true,
                    QuietHoursStart = new TimeSpan(22, 0, 0), // 10 PM
                    QuietHoursEnd = new TimeSpan(8, 0, 0),    // 8 AM
                    MinimumPriority = 2, // Normal priority and above
                    DailyLimit = 10
                };

                // Allow all notification types by default
                preferences.AllowedTypes.Add("alert_triggered");
                preferences.AllowedTypes.Add("reading_list_update");
                preferences.AllowedTypes.Add("general");
                preferences.AllowedTypes.Add("system");

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load user preferences for user {UserId}, using defaults", userId);
                return new NotificationPreferences { Enabled = true };
            }
        }

        private async Task<string> RenderAlertTemplate(NotificationMessage notification)
        {
            try
            {
                var model = new AlertEmailModel
                {
                    AlertName = notification.Metadata.GetValueOrDefault("AlertName", "").ToString() ?? "",
                    TriggerContext = notification.Metadata.GetValueOrDefault("TriggerContext", "").ToString() ?? "",
                    ArticleCount = (int)(notification.Metadata.GetValueOrDefault("ArticleCount", 0)),
                    Articles = notification.Metadata.GetValueOrDefault("Articles", new object[0]) as object[] ?? new object[0],
                    UserName = notification.Metadata.GetValueOrDefault("UserName", "").ToString() ?? "",
                    TriggeredAt = (DateTime)(notification.Metadata.GetValueOrDefault("TriggeredAt", DateTime.UtcNow))
                };

                // For now, return a simple HTML template - in production, use a proper template engine
                return GenerateAlertEmailHtml(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render alert email template, falling back to simple text");
                return notification.Body;
            }
        }

        private async Task<string> RenderReadingListTemplate(NotificationMessage notification)
        {
            try
            {
                var model = new ReadingListEmailModel
                {
                    ListName = notification.Metadata.GetValueOrDefault("ListName", "").ToString() ?? "",
                    NewArticleCount = (int)(notification.Metadata.GetValueOrDefault("NewArticleCount", 0)),
                    Articles = notification.Metadata.GetValueOrDefault("Articles", new object[0]) as object[] ?? new object[0],
                    UserName = notification.Metadata.GetValueOrDefault("UserName", "").ToString() ?? ""
                };

                // For now, return a simple HTML template
                return GenerateReadingListEmailHtml(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render reading list email template, falling back to simple text");
                return notification.Body;
            }
        }

        private static string GenerateAlertEmailHtml(AlertEmailModel model)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>NewsApp Alert: {model.AlertName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; }}
        .article {{ background-color: white; margin: 10px 0; padding: 15px; border-left: 4px solid #007bff; }}
        .article-title {{ font-weight: bold; margin-bottom: 5px; }}
        .article-source {{ color: #666; font-size: 0.9em; }}
        .footer {{ background-color: #343a40; color: white; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Alert Triggered: {model.AlertName}</h1>
            <p>Hello {model.UserName}, your alert has found {model.ArticleCount} new article(s)!</p>
        </div>
        <div class=""content"">
            <p><strong>Search Context:</strong> {model.TriggerContext}</p>
            <p><strong>Triggered At:</strong> {model.TriggeredAt:yyyy-MM-dd HH:mm} UTC</p>
            
            <h3>New Articles Found:</h3>
            {(model.ArticleCount > 0 ? 
                string.Join("", Enumerable.Range(0, Math.Min(model.ArticleCount, 5)).Select(i => 
                    $@"<div class=""article"">
                        <div class=""article-title"">Article {i + 1}</div>
                        <div class=""article-source"">Source information</div>
                       </div>")) 
                : "<p>No articles to display.</p>")}
            
            {(model.ArticleCount > 5 ? $"<p><em>... and {model.ArticleCount - 5} more articles. Login to NewsApp to see all results.</em></p>" : "")}
        </div>
        <div class=""footer"">
            <p>This is an automated notification from NewsApp. To manage your alert settings, please log in to your account.</p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GenerateReadingListEmailHtml(ReadingListEmailModel model)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>NewsApp: New Articles in {model.ListName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; }}
        .article {{ background-color: white; margin: 10px 0; padding: 15px; border-left: 4px solid #28a745; }}
        .footer {{ background-color: #343a40; color: white; padding: 15px; text-align: center; border-radius: 0 0 5px 5px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Reading List Update</h1>
            <p>Hello {model.UserName}, {model.NewArticleCount} new article(s) have been added to your list ""{model.ListName}""!</p>
        </div>
        <div class=""content"">
            <h3>New Articles:</h3>
            {(model.NewArticleCount > 0 ? 
                string.Join("", Enumerable.Range(0, Math.Min(model.NewArticleCount, 5)).Select(i => 
                    $@"<div class=""article"">New article {i + 1}</div>")) 
                : "<p>No articles to display.</p>")}
        </div>
        <div class=""footer"">
            <p>Login to NewsApp to read your new articles and manage your reading lists.</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    // Email template models
    public class AlertEmailModel
    {
        public string AlertName { get; set; } = string.Empty;
        public string TriggerContext { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public object[] Articles { get; set; } = new object[0];
        public string UserName { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
    }

    public class ReadingListEmailModel
    {
        public string ListName { get; set; } = string.Empty;
        public int NewArticleCount { get; set; }
        public object[] Articles { get; set; } = new object[0];
        public string UserName { get; set; } = string.Empty;
    }
}
