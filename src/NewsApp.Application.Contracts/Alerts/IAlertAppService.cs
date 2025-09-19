using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Alerts
{
    /// <summary>
    /// Application service for alert operations
    /// </summary>
    public interface IAlertAppService : IApplicationService
    {
        /// <summary>
        /// Get all alerts for the current user
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive alerts</param>
        /// <returns>List of user's alerts</returns>
        Task<List<AlertDto>> GetMyAlertsAsync(bool includeInactive = false);

        /// <summary>
        /// Get a specific alert by ID
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>Alert details</returns>
        Task<AlertDto> GetAsync(Guid id);

        /// <summary>
        /// Create a new alert
        /// </summary>
        /// <param name="input">Alert data</param>
        /// <returns>Created alert</returns>
        Task<AlertDto> CreateAsync(CreateAlertDto input);

        /// <summary>
        /// Update an existing alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="input">Updated alert data</param>
        /// <returns>Updated alert</returns>
        Task<AlertDto> UpdateAsync(Guid id, UpdateAlertDto input);

        /// <summary>
        /// Delete an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Activate an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        Task ActivateAsync(Guid id);

        /// <summary>
        /// Deactivate an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        Task DeactivateAsync(Guid id);

        /// <summary>
        /// Manually trigger an alert execution
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>True if execution was successful</returns>
        Task<bool> TriggerAsync(Guid id);

        /// <summary>
        /// Get results for a specific alert
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Alert execution results</returns>
        Task<PagedResultDto<AlertResultDto>> GetResultsAsync(Guid alertId, int skipCount = 0, int maxResultCount = 10);

        /// <summary>
        /// Get recent alert activity for the current user
        /// </summary>
        /// <param name="days">Number of days to look back</param>
        /// <returns>Recent alert activity</returns>
        Task<RecentAlertActivityDto> GetRecentActivityAsync(int days = 7);

        /// <summary>
        /// Get alert statistics for the current user
        /// </summary>
        /// <param name="since">Date to calculate statistics since (optional)</param>
        /// <returns>Alert statistics</returns>
        Task<AlertStatisticsDto> GetStatisticsAsync(DateTime? since = null);

        /// <summary>
        /// Get notifications from the last week
        /// </summary>
        /// <returns>Recent notifications</returns>
        Task<List<AlertResultDto>> GetRecentNotificationsAsync();

        /// <summary>
        /// Test an alert configuration without saving it
        /// </summary>
        /// <param name="input">Alert configuration to test</param>
        /// <returns>Test result with found articles count</returns>
        Task<AlertTestResultDto> TestAlertAsync(CreateAlertDto input);

        /// <summary>
        /// Get alerts by type
        /// </summary>
        /// <param name="alertType">Type of alerts to retrieve</param>
        /// <returns>Alerts of the specified type</returns>
        Task<List<AlertDto>> GetByTypeAsync(Domain.Alerts.AlertType alertType);

        /// <summary>
        /// Clone an existing alert
        /// </summary>
        /// <param name="id">Alert ID to clone</param>
        /// <param name="newName">Name for the cloned alert</param>
        /// <returns>Cloned alert</returns>
        Task<AlertDto> CloneAsync(Guid id, string newName);

        /// <summary>
        /// Update notification preferences for an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <param name="emailNotifications">Whether to send email notifications</param>
        /// <param name="minimumResultsToNotify">Minimum results to trigger notification</param>
        Task UpdateNotificationPreferencesAsync(Guid id, bool emailNotifications, int minimumResultsToNotify);

        /// <summary>
        /// Get alerts for a specific reading list
        /// </summary>
        /// <param name="listId">Reading list ID</param>
        /// <returns>Alerts configured for the list</returns>
        Task<List<AlertDto>> GetByListIdAsync(Guid listId);

        /// <summary>
        /// Bulk activate/deactivate alerts
        /// </summary>
        /// <param name="alertIds">Alert IDs to update</param>
        /// <param name="active">Whether to activate or deactivate</param>
        Task BulkUpdateActiveStatusAsync(List<Guid> alertIds, bool active);
    }
}
