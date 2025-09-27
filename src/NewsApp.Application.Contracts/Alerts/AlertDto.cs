using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using NewsApp.Domain.Alerts;

namespace NewsApp.Alerts
{
    /// <summary>
    /// DTO for alerts
    /// </summary>
    public class AlertDto : EntityDto<Guid>
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
        public string? QueryText { get; set; }

        /// <summary>
        /// Reading list ID (for List alerts)
        /// </summary>
        public Guid? ListId { get; set; }

        /// <summary>
        /// Whether this alert is currently active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// How frequently this alert should run
        /// </summary>
        public AlertFrequency Frequency { get; set; }

        /// <summary>
        /// When this alert was last executed
        /// </summary>
        public DateTime? LastRun { get; set; }

        /// <summary>
        /// When the last notification was sent for this alert
        /// </summary>
        public DateTime? LastNotificationSent { get; set; }

        /// <summary>
        /// Name/description of the alert
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Language code for search queries
        /// </summary>
        public string LanguageCode { get; set; } = "en";

        /// <summary>
        /// Whether to send email notifications
        /// </summary>
        public bool EmailNotifications { get; set; }

        /// <summary>
        /// Minimum number of results to trigger notification
        /// </summary>
        public int MinimumResultsToNotify { get; set; }

        /// <summary>
        /// When this alert was created
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Next scheduled run time
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// Name of the associated reading list (for List alerts)
        /// </summary>
        public string? ListName { get; set; }

        /// <summary>
        /// Owner's user name
        /// </summary>
        public string? OwnerUserName { get; set; }

        /// <summary>
        /// Keywords for the alert
        /// </summary>
        public string Keywords { get; set; } = string.Empty;

        /// <summary>
        /// Description of the alert
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether this alert is active
        /// </summary>
        public bool IsActive => Active;
    }

    /// <summary>
    /// DTO for creating alerts
    /// </summary>
    public class CreateAlertDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AlertType AlertType { get; set; }

        [Required]
        public AlertFrequency Frequency { get; set; } = AlertFrequency.Daily;

        [MaxLength(500)]
        public string? QueryText { get; set; }

        public Guid? ListId { get; set; }

        [MaxLength(10)]
        public string LanguageCode { get; set; } = "en";

        public bool EmailNotifications { get; set; } = true;

        [Range(1, 100)]
        public int MinimumResultsToNotify { get; set; } = 1;

        public bool Active { get; set; } = true;

        // Additional properties for enhanced functionality
        [MaxLength(500)]
        public string Keywords { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public AlertFrequencyUnit FrequencyUnit { get; set; } = AlertFrequencyUnit.Hours;

        public List<string>? SourceFilters { get; set; }

        public List<string>? CategoryFilters { get; set; }

        public List<string>? ExcludeKeywords { get; set; }

        public string? Language { get; set; }

        public List<string>? Country { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        [MaxLength(256)]
        public string? NotificationEmail { get; set; }

        public bool EmailNotificationEnabled { get; set; } = true;

        public bool PushNotificationEnabled { get; set; }

        public bool InAppNotificationEnabled { get; set; } = true;

        public bool ActivateImmediately { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating alerts
    /// </summary>
    public class UpdateAlertDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AlertFrequency Frequency { get; set; }

        [MaxLength(500)]
        public string? QueryText { get; set; }

        public Guid? ListId { get; set; }

        [MaxLength(10)]
        public string LanguageCode { get; set; } = "en";

        public bool EmailNotifications { get; set; }

        [Range(1, 100)]
        public int MinimumResultsToNotify { get; set; }

        public bool Active { get; set; }

        // Additional properties for enhanced functionality
        [MaxLength(500)]
        public string Keywords { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public AlertFrequencyUnit FrequencyUnit { get; set; } = AlertFrequencyUnit.Hours;

        public List<string>? SourceFilters { get; set; }

        public List<string>? CategoryFilters { get; set; }

        public List<string>? ExcludeKeywords { get; set; }

        public string? Language { get; set; }

        public List<string>? Country { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        [MaxLength(256)]
        public string? NotificationEmail { get; set; }

        public bool EmailNotificationEnabled { get; set; } = true;

        public bool PushNotificationEnabled { get; set; }

        public bool InAppNotificationEnabled { get; set; } = true;
    }

    /// <summary>
    /// DTO for alert results
    /// </summary>
    public class AlertResultDto : EntityDto<Guid>
    {
        /// <summary>
        /// ID of the alert that was executed
        /// </summary>
        public Guid AlertId { get; set; }

        /// <summary>
        /// When the alert was executed
        /// </summary>
        public DateTime RunAt { get; set; }

        /// <summary>
        /// Number of results found during execution
        /// </summary>
        public int FoundCount { get; set; }

        /// <summary>
        /// Whether the execution was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if execution failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Whether a notification was sent based on this result
        /// </summary>
        public bool NotificationSent { get; set; }

        /// <summary>
        /// When the notification was sent
        /// </summary>
        public DateTime? NotificationSentAt { get; set; }

        /// <summary>
        /// Name of the alert
        /// </summary>
        public string AlertName { get; set; } = string.Empty;

        /// <summary>
        /// Type of the alert
        /// </summary>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Execution status
        /// </summary>
        public AlertExecutionStatus Status { get; set; }

        /// <summary>
        /// Number of articles found
        /// </summary>
        public int ArticlesFound { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        public DateTime CreationTime { get; set; }
    }

    /// <summary>
    /// DTO for alert statistics
    /// </summary>
    public class AlertStatisticsDto
    {
        public Guid AlertId { get; set; }
        public string AlertName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int NotificationsSent { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime? LastExecution { get; set; }
        public double SuccessRate { get; set; }
        public double FailureRate { get; set; }
        public int TotalArticlesFound { get; set; }
        public double AverageArticlesPerExecution { get; set; }
        public DateTime? NextScheduledExecution { get; set; }
    }

    /// <summary>
    /// DTO for recent alert activity
    /// </summary>
    public class RecentAlertActivityDto
    {
        public AlertResultDto[] RecentResults { get; set; } = Array.Empty<AlertResultDto>();
        public AlertDto[] RecentlyTriggeredAlerts { get; set; } = Array.Empty<AlertDto>();
        public int TotalNotificationsThisWeek { get; set; }
        public DateTime LastActivityTime { get; set; }
    }

    /// <summary>
    /// DTO for alert with execution results
    /// </summary>
    public class AlertWithResultsDto : AlertDto
    {
        public AlertResultDto[] RecentResults { get; set; } = Array.Empty<AlertResultDto>();
    }

    /// <summary>
    /// DTO for alert execution result
    /// </summary>
    public class AlertExecutionResultDto
    {
        public Guid ExecutionId { get; set; }
        public bool Success { get; set; }
        public bool IsSuccess => Success;
        public string? Message { get; set; }
        public int ArticlesFound { get; set; }
        public DateTime ExecutionTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// DTO for importing alerts
    /// </summary>
    public class ImportAlertsDto
    {
        public string FileContent { get; set; } = string.Empty;
        public string Format { get; set; } = "json"; // json, csv, xml
        public bool OverwriteExisting { get; set; } = false;
        public bool ActivateImported { get; set; } = false;
        public List<ImportAlertItemDto> Alerts { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual alert import item
    /// </summary>
    public class ImportAlertItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AlertFrequency Frequency { get; set; }
        public AlertFrequencyUnit FrequencyUnit { get; set; }
        public List<string>? SourceFilters { get; set; }
        public List<string>? CategoryFilters { get; set; }
        public string? Language { get; set; }
    }

    /// <summary>
    /// DTO for exporting alerts
    /// </summary>
    public class ExportAlertsDto
    {
        public Guid[]? AlertIds { get; set; }
        public string Format { get; set; } = "json"; // json, csv, xml
        public bool IncludeResults { get; set; } = false;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// DTO for export alerts result
    /// </summary>
    public class ExportAlertsResultDto
    {
        public string Content { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int AlertCount { get; set; }
    }

    /// <summary>
    /// DTO for exporting individual alert
    /// </summary>
    public class ExportAlertDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public AlertType Type { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecuted { get; set; }
        public int TotalExecutions { get; set; }

        // Additional properties needed by AlertAppService
        public string Keywords { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AlertFrequency Frequency { get; set; }
        public AlertFrequencyUnit FrequencyUnit { get; set; }
        public List<string> SourceFilters { get; set; } = new();
        public List<string> CategoryFilters { get; set; } = new();
        public List<string> ExcludeKeywords { get; set; } = new();
        public string? Language { get; set; }
        public List<string> Country { get; set; } = new();
        public bool EmailNotificationEnabled { get; set; }
        public bool PushNotificationEnabled { get; set; }
        public bool InAppNotificationEnabled { get; set; }
    }

    /// <summary>
    /// DTO for alert test results
    /// </summary>
    public class AlertTestResultDto
    {
        public bool Success { get; set; }
        public int FoundCount { get; set; }
        public string? ErrorMessage { get; set; }
        public List<NewsArticlePreviewDto> SampleArticles { get; set; } = new();
        public DateTime TestedAt { get; set; }
    }

    /// <summary>
    /// DTO for news article preview in test results
    /// </summary>
    public class NewsArticlePreviewDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public string? Source { get; set; }
    }
}
