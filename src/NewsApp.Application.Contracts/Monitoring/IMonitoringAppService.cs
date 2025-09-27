using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Monitoring
{
    /// <summary>
    /// Application service for monitoring and metrics operations
    /// </summary>
    public interface IMonitoringAppService : IApplicationService
    {
        /// <summary>
        /// Get monitoring dashboard data
        /// </summary>
        /// <param name="days">Number of days to include in the dashboard (default: 7)</param>
        /// <returns>Dashboard data with metrics and statistics</returns>
        Task<MonitoringDashboardDto> GetDashboardAsync(int days = 7);

        /// <summary>
        /// Get API call statistics
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>API call statistics</returns>
        Task<ApiCallStatisticsDto> GetStatisticsAsync(MonitoringFilterDto? filter = null);

        /// <summary>
        /// Get recent API call metrics
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Recent API call metrics</returns>
        Task<PagedResultDto<ApiCallMetricDto>> GetRecentMetricsAsync(
            MonitoringFilterDto? filter = null, 
            int skipCount = 0, 
            int maxResultCount = 50);

        /// <summary>
        /// Get failed API calls
        /// </summary>
        /// <param name="since">Date to get failures since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Failed API calls</returns>
        Task<List<ApiCallMetricDto>> GetFailuresAsync(DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get slow API calls (above threshold)
        /// </summary>
        /// <param name="thresholdMs">Duration threshold in milliseconds</param>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Slow API calls</returns>
        Task<List<ApiCallMetricDto>> GetSlowCallsAsync(long thresholdMs = 5000, DateTime? since = null, int maxResultCount = 50);

        /// <summary>
        /// Get metrics grouped by time period
        /// </summary>
        /// <param name="since">Start date</param>
        /// <param name="until">End date</param>
        /// <param name="groupBy">Grouping period (hour, day, week)</param>
        /// <returns>Metrics grouped by time period</returns>
        Task<List<TimeGroupedMetricsDto>> GetMetricsByTimePeriodAsync(
            DateTime since, 
            DateTime until, 
            string groupBy = "day");

        /// <summary>
        /// Get daily metrics summary for the last N days
        /// </summary>
        /// <param name="days">Number of days to include</param>
        /// <returns>Daily metrics summary</returns>
        Task<List<DailyMetricsSummaryDto>> GetDailySummaryAsync(int days = 7);

        /// <summary>
        /// Get endpoint performance metrics
        /// </summary>
        /// <param name="since">Date to get metrics since</param>
        /// <returns>Performance metrics by endpoint</returns>
        Task<List<EndpointPerformanceMetricsDto>> GetEndpointPerformanceAsync(DateTime? since = null);

        /// <summary>
        /// Get metrics for a specific operation
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Metrics for the specified operation</returns>
        Task<List<ApiCallMetricDto>> GetByOperationAsync(string operation, DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get metrics for the current user
        /// </summary>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Metrics for the current user</returns>
        Task<List<ApiCallMetricDto>> GetMyMetricsAsync(DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get system health status
        /// </summary>
        /// <returns>System health information</returns>
        Task<SystemHealthDto> GetSystemHealthAsync();

        /// <summary>
        /// Clean up old metrics (admin only)
        /// </summary>
        /// <param name="olderThanDays">Delete metrics older than this many days</param>
        /// <returns>Number of records deleted</returns>
        Task<int> CleanupOldMetricsAsync(int olderThanDays = 90);

        /// <summary>
        /// Export metrics data
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <param name="format">Export format (json, csv)</param>
        /// <returns>Exported metrics data</returns>
        Task<string> ExportMetricsAsync(MonitoringFilterDto? filter = null, string format = "json");

        /// <summary>
        /// Get real-time metrics (last hour)
        /// </summary>
        /// <returns>Real-time metrics data</returns>
        Task<RealTimeMetricsDto> GetRealTimeMetricsAsync();

        /// <summary>
        /// Test external service connectivity
        /// </summary>
        /// <returns>Connectivity test results</returns>
        Task<List<ServiceConnectivityDto>> TestConnectivityAsync();
    }

    /// <summary>
    /// DTO for system health status
    /// </summary>
    public class SystemHealthDto
    {
        public bool IsHealthy { get; set; }
        public DateTime CheckedAt { get; set; }
        public List<HealthCheckDto> Checks { get; set; } = new();
        public double OverallScore { get; set; }
        public string Status { get; set; } = string.Empty; // Healthy, Degraded, Unhealthy
    }

    /// <summary>
    /// DTO for individual health checks
    /// </summary>
    public class HealthCheckDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string? ErrorMessage { get; set; }
        public long ResponseTimeMs { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }

    /// <summary>
    /// DTO for real-time metrics
    /// </summary>
    public class RealTimeMetricsDto
    {
        public int CallsLastHour { get; set; }
        public int CallsLastMinute { get; set; }
        public double AverageResponseTime { get; set; }
        public double SuccessRate { get; set; }
        public int ActiveRequests { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<TimeGroupedMetricsDto> Last60Minutes { get; set; } = new();
    }

    /// <summary>
    /// DTO for service connectivity tests
    /// </summary>
    public class ServiceConnectivityDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime TestedAt { get; set; }
    }
}
