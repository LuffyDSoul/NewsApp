using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsApp.Domain.Notifications.Services
{
    /// <summary>
    /// Interface for notification channels - allows pluggable implementations
    /// </summary>
    public interface INotificationChannel
    {
        /// <summary>
        /// Name of the notification channel
        /// </summary>
        string ChannelName { get; }

        /// <summary>
        /// Send a notification to a user
        /// </summary>
        /// <param name="notification">Notification details</param>
        /// <returns>True if notification was sent successfully</returns>
        Task<bool> SendAsync(NotificationMessage notification);

        /// <summary>
        /// Test if the notification channel is available and configured correctly
        /// </summary>
        /// <returns>True if channel is available</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Get notification delivery preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's preferences for this channel</returns>
        Task<NotificationPreferences> GetUserPreferencesAsync(Guid userId);
    }

    /// <summary>
    /// Represents a notification message
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// Recipient user ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Recipient email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Notification subject/title
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Notification body/content
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// HTML version of the body (for email)
        /// </summary>
        public string? HtmlBody { get; set; }

        /// <summary>
        /// Notification type/category
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Priority level (1 = Low, 2 = Normal, 3 = High, 4 = Urgent)
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// When the notification should be sent (for scheduled notifications)
        /// </summary>
        public DateTime? ScheduledFor { get; set; }

        public static NotificationMessage Create(Guid userId, string email, string subject, string body, string type = "general")
        {
            return new NotificationMessage
            {
                UserId = userId,
                Email = email,
                Subject = subject,
                Body = body,
                Type = type
            };
        }

        public NotificationMessage WithHtml(string htmlBody)
        {
            HtmlBody = htmlBody;
            return this;
        }

        public NotificationMessage WithPriority(int priority)
        {
            Priority = Math.Max(1, Math.Min(4, priority));
            return this;
        }

        public NotificationMessage WithMetadata(string key, object value)
        {
            Metadata[key] = value;
            return this;
        }

        public NotificationMessage ScheduleFor(DateTime scheduleTime)
        {
            ScheduledFor = scheduleTime;
            return this;
        }
    }

    /// <summary>
    /// User preferences for notifications
    /// </summary>
    public class NotificationPreferences
    {
        /// <summary>
        /// Whether notifications are enabled for this channel
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Quiet hours start time (24-hour format)
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end time (24-hour format)
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// Types of notifications to receive
        /// </summary>
        public HashSet<string> AllowedTypes { get; set; } = new();

        /// <summary>
        /// Minimum priority level to receive notifications
        /// </summary>
        public int MinimumPriority { get; set; } = 1;

        /// <summary>
        /// Maximum number of notifications per day
        /// </summary>
        public int? DailyLimit { get; set; }

        public bool ShouldReceive(NotificationMessage notification)
        {
            if (!Enabled)
                return false;

            if (notification.Priority < MinimumPriority)
                return false;

            if (AllowedTypes.Any() && !AllowedTypes.Contains(notification.Type))
                return false;

            // Check quiet hours
            if (QuietHoursStart.HasValue && QuietHoursEnd.HasValue)
            {
                var now = DateTime.Now.TimeOfDay;
                if (QuietHoursStart <= QuietHoursEnd)
                {
                    // Same day quiet hours
                    if (now >= QuietHoursStart && now <= QuietHoursEnd)
                        return false;
                }
                else
                {
                    // Overnight quiet hours
                    if (now >= QuietHoursStart || now <= QuietHoursEnd)
                        return false;
                }
            }

            return true;
        }
    }
}
