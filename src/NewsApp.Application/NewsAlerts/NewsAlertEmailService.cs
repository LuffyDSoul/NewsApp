using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewsApp.Domain.NewsAlerts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace NewsApp.NewsAlerts
{
    public interface INewsAlertEmailService
    {
        Task SendNewAlertsEmailAsync(Guid userId, List<NewsAlertNotification> notifications);
    }

    public class NewsAlertEmailService : INewsAlertEmailService, ITransientDependency
    {
        private readonly IEmailSender _emailSender;
        private readonly IIdentityUserRepository _userRepository;
        private readonly INewsAlertNotificationRepository _notificationRepository;
        private readonly ILogger<NewsAlertEmailService> _logger;

        public NewsAlertEmailService(
            IEmailSender emailSender,
            IIdentityUserRepository userRepository,
            INewsAlertNotificationRepository notificationRepository,
            ILogger<NewsAlertEmailService> logger)
        {
            _emailSender = emailSender;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task SendNewAlertsEmailAsync(Guid userId, List<NewsAlertNotification> notifications)
        {
            try
            {
                var user = await _userRepository.GetAsync(userId);
                
                if (string.IsNullOrEmpty(user.Email))
                {
                    _logger.LogWarning("User {UserId} has no email address", userId);
                    return;
                }

                var subject = $"You have {notifications.Sum(n => n.NewArticlesCount)} new news alerts";
                var body = BuildEmailBody(notifications);

                await _emailSender.SendAsync(
                    user.Email,
                    subject,
                    body
                );

                // Mark emails as sent
                foreach (var notification in notifications)
                {
                    notification.MarkEmailSent();
                    await _notificationRepository.UpdateAsync(notification);
                }

                _logger.LogInformation("Successfully sent alert email to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert email to user {UserId}", userId);
                throw;
            }
        }

        private string BuildEmailBody(List<NewsAlertNotification> notifications)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine("        .header { background-color: #007bff; color: white; padding: 20px; text-align: center; }");
            sb.AppendLine("        .notification { background-color: #f8f9fa; margin: 15px 0; padding: 15px; border-left: 4px solid #007bff; }");
            sb.AppendLine("        .notification-title { font-weight: bold; font-size: 1.1em; margin-bottom: 10px; }");
            sb.AppendLine("        .notification-details { color: #666; font-size: 0.9em; }");
            sb.AppendLine("        .footer { margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #666; }");
            sb.AppendLine("        .btn { display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin-top: 10px; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class='container'>");
            sb.AppendLine("        <div class='header'>");
            sb.AppendLine("            <h1>📰 New News Alerts</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div style='padding: 20px;'>");
            sb.AppendLine($"            <p>You have received {notifications.Count} new alert notification(s) with a total of {notifications.Sum(n => n.NewArticlesCount)} new articles.</p>");
            
            foreach (var notification in notifications.OrderBy(n => n.AlertListName).ThenBy(n => n.Category))
            {
                sb.AppendLine("            <div class='notification'>");
                sb.AppendLine($"                <div class='notification-title'>{notification.AlertListName} - {notification.Category}</div>");
                sb.AppendLine("                <div class='notification-details'>");
                sb.AppendLine($"                    <strong>{notification.NewArticlesCount}</strong> new article(s) found<br>");
                sb.AppendLine($"                    Language: <strong>{notification.LanguageCode.ToUpper()}</strong><br>");
                sb.AppendLine($"                    Latest article: {notification.NewestArticleDate:MMM dd, yyyy HH:mm} UTC");
                sb.AppendLine("                </div>");
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("            <div style='text-align: center; margin-top: 30px;'>");
            sb.AppendLine("                <a href='http://localhost:4200' class='btn'>View All News</a>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class='footer'>");
            sb.AppendLine("            <p>You are receiving this email because you have active news alerts configured.</p>");
            sb.AppendLine("            <p>To manage your alerts, please log in to NewsApp.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
