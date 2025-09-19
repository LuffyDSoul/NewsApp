using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;
using NewsApp.News;

namespace NewsApp.Domain.Alerts.Events
{
    /// <summary>
    /// Domain event fired when an alert is triggered
    /// </summary>
    [EventName("NewsApp.AlertTriggered")]
    public class AlertTriggeredEvent : EtoBase
    {
        /// <summary>
        /// ID of the alert that was triggered
        /// </summary>
        public Guid AlertId { get; set; }

        /// <summary>
        /// ID of the user who owns the alert
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Type of alert that was triggered
        /// </summary>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Name of the alert
        /// </summary>
        public string AlertName { get; set; } = string.Empty;

        /// <summary>
        /// Query text (for search alerts) or list name (for list alerts)
        /// </summary>
        public string TriggerContext { get; set; } = string.Empty;

        /// <summary>
        /// Articles that triggered the alert
        /// </summary>
        public List<ArticleReference> TriggeringArticles { get; set; } = new();

        /// <summary>
        /// When the alert was triggered
        /// </summary>
        public DateTime TriggeredAt { get; set; }

        /// <summary>
        /// Number of articles that triggered the alert
        /// </summary>
        public int ArticleCount { get; set; }

        /// <summary>
        /// Whether email notification should be sent
        /// </summary>
        public bool SendEmailNotification { get; set; }

        public AlertTriggeredEvent()
        {
            TriggeredAt = DateTime.UtcNow;
        }

        public AlertTriggeredEvent(
            Guid alertId,
            Guid userId,
            AlertType alertType,
            string alertName,
            string triggerContext,
            List<ArticleReference> triggeringArticles,
            bool sendEmailNotification = true) : this()
        {
            AlertId = alertId;
            UserId = userId;
            AlertType = alertType;
            AlertName = alertName;
            TriggerContext = triggerContext;
            TriggeringArticles = triggeringArticles;
            ArticleCount = triggeringArticles.Count;
            SendEmailNotification = sendEmailNotification;
        }
    }
}
