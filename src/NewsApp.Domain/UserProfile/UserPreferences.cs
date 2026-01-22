using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.UserProfile
{
    /// <summary>
    /// User preferences entity for storing user-specific settings
    /// </summary>
    public class UserPreferences : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// ID of the user these preferences belong to
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// User's preferred language code for news content
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string NewsLanguageCode { get; set; } = "en";

        /// <summary>
        /// User's preferred language name for display
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string NewsLanguageName { get; set; } = "English";

        /// <summary>
        /// Whether to receive email notifications
        /// </summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>
        /// Whether to receive push notifications
        /// </summary>
        public bool PushNotifications { get; set; } = false;

        /// <summary>
        /// Maximum number of articles per page
        /// </summary>
        public int ArticlesPerPage { get; set; } = 20;

        /// <summary>
        /// User's timezone preference
        /// </summary>
        [MaxLength(100)]
        public string? TimeZone { get; set; }

        /// <summary>
        /// Theme preference (light/dark/auto)
        /// </summary>
        [MaxLength(20)]
        public string Theme { get; set; } = "auto";

        /// <summary>
        /// Whether to show images in article listings
        /// </summary>
        public bool ShowImages { get; set; } = true;

        /// <summary>
        /// Whether to auto-refresh news feed
        /// </summary>
        public bool AutoRefresh { get; set; } = false;

        /// <summary>
        /// Auto-refresh interval in minutes
        /// </summary>
        public int AutoRefreshInterval { get; set; } = 30;

        protected UserPreferences()
        {
            // For EF Core
        }

        public UserPreferences(Guid id, Guid userId, string newsLanguageCode = "en", string newsLanguageName = "English") : base(id)
        {
            UserId = userId;
            SetNewsLanguage(newsLanguageCode, newsLanguageName);
        }

        /// <summary>
        /// Set the news language preference
        /// </summary>
        /// <param name="languageCode">Language code</param>
        /// <param name="languageName">Language display name</param>
        public void SetNewsLanguage(string languageCode, string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code cannot be empty", nameof(languageCode));

            if (string.IsNullOrWhiteSpace(languageName))
                throw new ArgumentException("Language name cannot be empty", nameof(languageName));

            NewsLanguageCode = languageCode.ToLowerInvariant();
            NewsLanguageName = languageName;
        }

        /// <summary>
        /// Update notification preferences
        /// </summary>
        /// <param name="emailNotifications">Enable email notifications</param>
        /// <param name="pushNotifications">Enable push notifications</param>
        public void SetNotificationPreferences(bool emailNotifications, bool pushNotifications)
        {
            EmailNotifications = emailNotifications;
            PushNotifications = pushNotifications;
        }

        /// <summary>
        /// Set display preferences
        /// </summary>
        /// <param name="articlesPerPage">Articles per page (between 10 and 100)</param>
        /// <param name="showImages">Show images in listings</param>
        /// <param name="theme">Theme preference</param>
        public void SetDisplayPreferences(int articlesPerPage, bool showImages, string theme)
        {
            if (articlesPerPage < 10 || articlesPerPage > 100)
                throw new ArgumentException("Articles per page must be between 10 and 100", nameof(articlesPerPage));

            ArticlesPerPage = articlesPerPage;
            ShowImages = showImages;
            Theme = theme ?? "auto";
        }

        /// <summary>
        /// Set auto-refresh preferences
        /// </summary>
        /// <param name="autoRefresh">Enable auto-refresh</param>
        /// <param name="intervalMinutes">Refresh interval in minutes (between 5 and 120)</param>
        public void SetAutoRefreshPreferences(bool autoRefresh, int intervalMinutes = 30)
        {
            if (intervalMinutes < 5 || intervalMinutes > 120)
                throw new ArgumentException("Auto-refresh interval must be between 5 and 120 minutes", nameof(intervalMinutes));

            AutoRefresh = autoRefresh;
            AutoRefreshInterval = intervalMinutes;
        }
    }
}