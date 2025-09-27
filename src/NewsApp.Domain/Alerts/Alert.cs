using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using NewsApp.Domain.Alerts;

namespace NewsApp.Domain.Alerts
{
    /// <summary>
    /// Represents an alert configuration for a user
    /// </summary>
    public class Alert : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// ID of the user who owns this alert
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Type of alert (SearchQuery or List)
        /// </summary>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Search query text (for SearchQuery alerts)
        /// </summary>
        [MaxLength(500)]
        public string? QueryText { get; set; }

        /// <summary>
        /// Reading list ID (for List alerts)
        /// </summary>
        public Guid? ListId { get; set; }

        /// <summary>
        /// Whether this alert is currently active
        /// </summary>
        public bool Active { get; set; } = true;

        /// <summary>
        /// How frequently this alert should run
        /// </summary>
        public AlertFrequency Frequency { get; set; } = AlertFrequency.Daily;

        /// <summary>
        /// When this alert was last executed
        /// </summary>
        public DateTime? LastRun { get; set; }

        /// <summary>
        /// When the last notification was sent for this alert
        /// </summary>
        public DateTime? LastNotificationSent { get; set; }

        /// <summary>
        /// Name/description of the alert for user identification
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Language code for search queries
        /// </summary>
        [MaxLength(10)]
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Whether to send email notifications
        /// </summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>
        /// Minimum number of results to trigger notification (to avoid spam)
        /// </summary>
        public int MinimumResultsToNotify { get; set; } = 1;

        /// <summary>
        /// Keywords for the alert
        /// </summary>
        [MaxLength(500)]
        public string Keywords { get; set; } = string.Empty;

        /// <summary>
        /// Description of the alert
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Frequency unit (used with numeric frequency)
        /// </summary>
        public AlertFrequencyUnit FrequencyUnit { get; set; } = AlertFrequencyUnit.Hours;

        /// <summary>
        /// Source filters for the alert
        /// </summary>
        public List<string> SourceFilters { get; set; } = new();

        /// <summary>
        /// Category filters for the alert
        /// </summary>
        public List<string> CategoryFilters { get; set; } = new();

        /// <summary>
        /// Keywords to exclude from results
        /// </summary>
        public List<string> ExcludeKeywords { get; set; } = new();

        /// <summary>
        /// Language filter for articles
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Country filters for the alert
        /// </summary>
        public List<string> Country { get; set; } = new();

        /// <summary>
        /// Date from filter
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Date to filter
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Email for notifications
        /// </summary>
        [MaxLength(256)]
        public string? NotificationEmail { get; set; }

        /// <summary>
        /// Whether push notifications are enabled
        /// </summary>
        public bool PushNotificationEnabled { get; set; }

        /// <summary>
        /// Whether email notifications are enabled
        /// </summary>
        public bool EmailNotificationEnabled { get; set; } = true;

        /// <summary>
        /// Whether in-app notifications are enabled
        /// </summary>
        public bool InAppNotificationEnabled { get; set; } = true;

        /// <summary>
        /// Next scheduled run time
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// Whether this alert is active (alias for Active property)
        /// </summary>
        public bool IsActive => Active;

        protected Alert()
        {
            // For EF Core
        }

        public Alert(
            Guid id,
            string name,
            string keywords,
            Guid userId,
            string? description = null) : base(id)
        {
            SetBasicInfo(name, keywords, description);
            UserId = userId;
        }

        public Alert(
            Guid id,
            Guid userId,
            string name,
            AlertType alertType,
            AlertFrequency frequency = AlertFrequency.Daily,
            string? queryText = null,
            Guid? listId = null,
            string languageCode = "en") : base(id)
        {
            SetBasicInfo(userId, name, alertType, frequency, languageCode);
            
            if (alertType == AlertType.SearchQuery)
            {
                SetSearchQuery(queryText);
            }
            else if (alertType == AlertType.List)
            {
                SetListId(listId);
            }
        }

        public void SetBasicInfo(string name, string keywords, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Alert name cannot be empty", nameof(name));

            Name = name;
            Keywords = keywords ?? string.Empty;
            Description = description;
        }

        public void SetBasicInfo(Guid userId, string name, AlertType alertType, AlertFrequency frequency, string languageCode = "en")
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Alert name cannot be empty", nameof(name));

            UserId = userId;
            Name = name;
            AlertType = alertType;
            Frequency = frequency;
            LanguageCode = languageCode;
        }

        public void SetFrequency(AlertFrequency frequency, AlertFrequencyUnit unit)
        {
            Frequency = frequency;
            FrequencyUnit = unit;
        }

        public void SetSourceFilters(IEnumerable<string> sources)
        {
            SourceFilters = sources?.ToList() ?? new List<string>();
        }

        public void SetCategoryFilters(IEnumerable<string> categories)
        {
            CategoryFilters = categories?.ToList() ?? new List<string>();
        }

        public void SetExcludeKeywords(IEnumerable<string> keywords)
        {
            ExcludeKeywords = keywords?.ToList() ?? new List<string>();
        }

        public void SetLanguage(string language)
        {
            Language = language;
        }

        public void ClearLanguage()
        {
            Language = null;
        }

        public void SetCountryFilter(IEnumerable<string> countries)
        {
            Country = countries?.ToList() ?? new List<string>();
        }

        public void SetDateRange(DateTime from, DateTime? to = null)
        {
            DateFrom = from;
            DateTo = to;
        }

        public void ClearDateRange()
        {
            DateFrom = null;
            DateTo = null;
        }

        public void EnableEmailNotifications(string? email = null)
        {
            EmailNotificationEnabled = true;
            if (!string.IsNullOrWhiteSpace(email))
            {
                NotificationEmail = email;
            }
        }

        public void DisableEmailNotifications()
        {
            EmailNotificationEnabled = false;
        }

        public void EnablePushNotifications()
        {
            PushNotificationEnabled = true;
        }

        public void DisablePushNotifications()
        {
            PushNotificationEnabled = false;
        }

        public void EnableInAppNotifications()
        {
            InAppNotificationEnabled = true;
        }

        public void DisableInAppNotifications()
        {
            InAppNotificationEnabled = false;
        }

        public bool HasNotificationsEnabled()
        {
            return EmailNotificationEnabled || PushNotificationEnabled || InAppNotificationEnabled;
        }

        public void SetSearchQuery(string? queryText)
        {
            if (AlertType != AlertType.SearchQuery)
                throw new InvalidOperationException("Cannot set search query for non-search alerts");

            if (string.IsNullOrWhiteSpace(queryText))
                throw new ArgumentException("Query text cannot be empty for search alerts", nameof(queryText));

            QueryText = queryText;
            ListId = null;
        }

        public void SetListId(Guid? listId)
        {
            if (AlertType != AlertType.List)
                throw new InvalidOperationException("Cannot set list ID for non-list alerts");

            if (listId == null || listId == Guid.Empty)
                throw new ArgumentException("List ID cannot be empty for list alerts", nameof(listId));

            ListId = listId;
            QueryText = null;
        }

        public void Activate()
        {
            Active = true;
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void UpdateLastRun(DateTime runTime)
        {
            LastRun = runTime;
        }

        public void UpdateLastNotificationSent(DateTime sentTime)
        {
            LastNotificationSent = sentTime;
        }

        public void RecordExecution(DateTime executionTime)
        {
            LastRun = executionTime;
            NextRunTime = GetNextRunTime();
        }

        public void SetNextRunTime(DateTime nextRun)
        {
            NextRunTime = nextRun;
        }

        public void ResetNextRunTime()
        {
            NextRunTime = GetNextRunTime();
        }

        public bool ShouldRun()
        {
            if (!Active)
                return false;

            if (LastRun == null)
                return true;

            var nextRun = GetNextRunTime();
            return DateTime.UtcNow >= nextRun;
        }

        public DateTime GetNextRunTime()
        {
            if (LastRun == null)
                return DateTime.UtcNow;

            return Frequency switch
            {
                AlertFrequency.EveryFifteenMinutes => LastRun.Value.AddMinutes(15),
                AlertFrequency.Hourly => LastRun.Value.AddHours(1),
                AlertFrequency.SixHourly => LastRun.Value.AddHours(6),
                AlertFrequency.Daily => LastRun.Value.AddDays(1),
                AlertFrequency.Weekly => LastRun.Value.AddDays(7),
                _ => LastRun.Value.AddDays(1)
            };
        }

        public void SetNotificationPreferences(bool emailNotifications, int minimumResultsToNotify = 1)
        {
            EmailNotifications = emailNotifications;
            MinimumResultsToNotify = Math.Max(1, minimumResultsToNotify);
        }
    }
}
