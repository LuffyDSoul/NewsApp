using System;
using System.Threading.Tasks;

namespace NewsApp.Domain.Alerts.Services
{
    /// <summary>
    /// Interface for alert scheduling service
    /// </summary>
    public interface IAlertScheduler
    {
        /// <summary>
        /// Execute all alerts that are due to run
        /// </summary>
        /// <returns>Number of alerts processed</returns>
        Task<int> ExecuteDueAlertsAsync();

        /// <summary>
        /// Execute a specific alert
        /// </summary>
        /// <param name="alertId">ID of the alert to execute</param>
        /// <returns>True if execution was successful</returns>
        Task<bool> ExecuteAlertAsync(Guid alertId);

        /// <summary>
        /// Schedule an alert for future execution
        /// </summary>
        /// <param name="alertId">ID of the alert to schedule</param>
        /// <param name="executeAt">When to execute the alert</param>
        /// <returns>True if scheduling was successful</returns>
        Task<bool> ScheduleAlertAsync(Guid alertId, DateTime executeAt);

        /// <summary>
        /// Cancel a scheduled alert
        /// </summary>
        /// <param name="alertId">ID of the alert to cancel</param>
        /// <returns>True if cancellation was successful</returns>
        Task<bool> CancelScheduledAlertAsync(Guid alertId);

        /// <summary>
        /// Get the next execution time for an alert
        /// </summary>
        /// <param name="alertId">ID of the alert</param>
        /// <returns>Next execution time, or null if not scheduled</returns>
        Task<DateTime?> GetNextExecutionTimeAsync(Guid alertId);
    }
}
