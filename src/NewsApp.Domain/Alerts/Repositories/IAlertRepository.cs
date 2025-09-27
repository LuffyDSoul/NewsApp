using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using NewsApp.Domain.Alerts;

namespace NewsApp.Domain.Alerts.Repositories
{
    /// <summary>
    /// Repository interface for Alert entities
    /// </summary>
    public interface IAlertRepository : IRepository<Alert, Guid>
    {
        /// <summary>
        /// Count active alerts
        /// </summary>
        /// <returns>Number of active alerts</returns>
        Task<int> CountActiveAsync();

        /// <summary>
        /// Get all alerts for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeInactive">Whether to include inactive alerts</param>
        /// <returns>List of alerts owned by the user</returns>
        Task<List<Alert>> GetByUserIdAsync(Guid userId, bool includeInactive = false);

        /// <summary>
        /// Get alerts that are due to run
        /// </summary>
        /// <returns>List of alerts that should be executed</returns>
        Task<List<Alert>> GetDueAlertsAsync();

        /// <summary>
        /// Get alerts by type
        /// </summary>
        /// <param name="alertType">Type of alert</param>
        /// <param name="userId">User ID (optional)</param>
        /// <returns>Alerts of the specified type</returns>
        Task<List<Alert>> GetByTypeAsync(AlertType alertType, Guid? userId = null);

        /// <summary>
        /// Get alerts for a specific reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <returns>Alerts configured for the specified list</returns>
        Task<List<Alert>> GetByListIdAsync(Guid listId);

        /// <summary>
        /// Search alerts by query text
        /// </summary>
        /// <param name="queryText">Text to search for</param>
        /// <param name="userId">User ID (optional)</param>
        /// <returns>Alerts matching the query text</returns>
        Task<List<Alert>> SearchByQueryAsync(string queryText, Guid? userId = null);

        /// <summary>
        /// Get alerts by frequency
        /// </summary>
        /// <param name="frequency">Alert frequency</param>
        /// <returns>Alerts with the specified frequency</returns>
        Task<List<Alert>> GetByFrequencyAsync(AlertFrequency frequency);

        /// <summary>
        /// Get recently triggered alerts for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="since">Date to get alerts since</param>
        /// <returns>Recently triggered alerts</returns>
        Task<List<Alert>> GetRecentlyTriggeredAsync(Guid userId, DateTime since);

        /// <summary>
        /// Get alerts that haven't run for a specified period
        /// </summary>
        /// <param name="olderThan">Date threshold</param>
        /// <returns>Alerts that haven't run since the specified date</returns>
        Task<List<Alert>> GetStaleAlertsAsync(DateTime olderThan);

        /// <summary>
        /// Update last run time for multiple alerts
        /// </summary>
        /// <param name="alertUpdates">Dictionary of alert ID to last run time</param>
        Task UpdateLastRunTimesAsync(Dictionary<Guid, DateTime> alertUpdates);

        // Additional methods needed by AlertAppService
        /// <summary>
        /// Get paged list of alerts for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <param name="sorting">Sort criteria</param>
        /// <returns>Paged list of alerts</returns>
        Task<List<Alert>> GetPagedListAsync(Guid userId, int skipCount, int maxResultCount, string? sorting = null);

        /// <summary>
        /// Count alerts for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of alerts</returns>
        Task<int> CountAsync(Guid userId);

        /// <summary>
        /// Get active alerts for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Active alerts</returns>
        Task<List<Alert>> GetActiveByUserIdAsync(Guid userId);

        /// <summary>
        /// Get alerts by keyword for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="keyword">Keyword to search</param>
        /// <returns>Alerts matching the keyword</returns>
        Task<List<Alert>> GetByKeywordAsync(Guid userId, string keyword);

        /// <summary>
        /// Get alerts by IDs
        /// </summary>
        /// <param name="alertIds">Alert IDs</param>
        /// <returns>Alerts with the specified IDs</returns>
        Task<List<Alert>> GetByIdsAsync(Guid[] alertIds);

        /// <summary>
        /// Save changes to the repository
        /// </summary>
        /// <returns>Task</returns>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// Repository interface for AlertResult entities
    /// </summary>
    public interface IAlertResultRepository : IRepository<AlertResult, Guid>
    {
        /// <summary>
        /// Get results for a specific alert
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Results for the specified alert</returns>
        Task<List<AlertResult>> GetByAlertIdAsync(Guid alertId, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get recent alert results for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="since">Date to get results since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Recent alert results</returns>
        Task<List<AlertResult>> GetRecentByUserIdAsync(Guid userId, DateTime since, int maxResultCount = 50);

        /// <summary>
        /// Get failed alert results
        /// </summary>
        /// <param name="since">Date to get failures since</param>
        /// <returns>Failed alert results</returns>
        Task<List<AlertResult>> GetFailuresAsync(DateTime since);

        /// <summary>
        /// Get alert results that triggered notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="since">Date to get results since</param>
        /// <returns>Results that triggered notifications</returns>
        Task<List<AlertResult>> GetTriggeredNotificationsAsync(Guid userId, DateTime since);

        /// <summary>
        /// Get statistics for alerts
        /// </summary>
        /// <param name="userId">User ID (optional)</param>
        /// <param name="since">Date to calculate statistics since</param>
        /// <returns>Alert statistics</returns>
        Task<AlertStatistics> GetStatisticsAsync(Guid? userId = null, DateTime? since = null);

        /// <summary>
        /// Clean up old alert results
        /// </summary>
        /// <param name="olderThan">Date threshold for cleanup</param>
        /// <returns>Number of records deleted</returns>
        Task<int> CleanupOldResultsAsync(DateTime olderThan);

        /// <summary>
        /// Get results by alert ID in a specific period
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Alert results in the specified period</returns>
        Task<List<AlertResult>> GetByAlertIdInPeriodAsync(Guid alertId, DateTime fromDate, DateTime toDate);
    }

    /// <summary>
    /// Statistics for alert execution
    /// </summary>
    public class AlertStatistics
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public int NotificationsSent { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime? LastExecution { get; set; }
        
        public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions * 100 : 0;
        public double FailureRate => TotalExecutions > 0 ? (double)FailedExecutions / TotalExecutions * 100 : 0;
    }
}
