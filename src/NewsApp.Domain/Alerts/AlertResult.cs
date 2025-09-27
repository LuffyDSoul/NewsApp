using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.Alerts
{
    /// <summary>
    /// Represents the result of an alert execution
    /// </summary>
    public class AlertResult : AuditedEntity<Guid>
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
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// JSON details of the results (optional, for debugging or detailed logging)
        /// </summary>
        public string? DetailsJson { get; set; }

        /// <summary>
        /// Whether a notification was sent based on this result
        /// </summary>
        public bool NotificationSent { get; set; }

        /// <summary>
        /// When the notification was sent (if applicable)
        /// </summary>
        public DateTime? NotificationSentAt { get; set; }

        /// <summary>
        /// Navigation property to the alert
        /// </summary>
        public virtual Alert Alert { get; set; } = null!;

        /// <summary>
        /// Execution status
        /// </summary>
        public AlertExecutionStatus Status { get; set; }

        /// <summary>
        /// Number of articles found (alias for FoundCount)
        /// </summary>
        public int ArticlesFound => FoundCount;

        /// <summary>
        /// Execution duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        protected AlertResult()
        {
            // For EF Core
        }

        public AlertResult(
            Guid id,
            Guid alertId,
            DateTime runAt,
            int foundCount,
            bool success = true,
            string? errorMessage = null,
            string? detailsJson = null) : base(id)
        {
            AlertId = alertId;
            RunAt = runAt;
            FoundCount = foundCount;
            Success = success;
            ErrorMessage = errorMessage;
            DetailsJson = detailsJson;
            Status = success ? AlertExecutionStatus.Success : AlertExecutionStatus.Failed;
        }

        public AlertResult(
            Guid id,
            Guid alertId,
            AlertExecutionStatus status,
            int articlesFound,
            TimeSpan duration,
            string? errorMessage = null) : base(id)
        {
            AlertId = alertId;
            RunAt = DateTime.UtcNow;
            FoundCount = articlesFound;
            Success = status == AlertExecutionStatus.Success;
            ErrorMessage = errorMessage;
            Status = status;
            Duration = duration;
        }

        public static AlertResult CreateSuccess(Guid alertId, DateTime runAt, int foundCount, string? detailsJson = null)
        {
            return new AlertResult(Guid.NewGuid(), alertId, runAt, foundCount, true, null, detailsJson);
        }

        public static AlertResult CreateFailure(Guid alertId, DateTime runAt, string errorMessage)
        {
            return new AlertResult(Guid.NewGuid(), alertId, runAt, 0, false, errorMessage);
        }

        public void MarkNotificationSent(DateTime sentAt)
        {
            NotificationSent = true;
            NotificationSentAt = sentAt;
        }
    }
}
