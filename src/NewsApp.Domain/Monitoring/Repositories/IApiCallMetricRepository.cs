using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using NewsApp.Domain.Monitoring;

namespace NewsApp.Domain.Monitoring.Repositories
{
    /// <summary>
    /// Repository interface for ApiCallMetric entities
    /// </summary>
    public interface IApiCallMetricRepository : IRepository<ApiCallMetric, Guid>
    {
        /// <summary>
        /// Get metrics in a specific time period
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="endpoint">Optional endpoint filter</param>
        /// <returns>Metrics in the specified period</returns>
        Task<List<ApiCallMetric>> GetInPeriodAsync(DateTime fromDate, DateTime toDate, string? endpoint = null);

        /// <summary>
        /// Count metrics in a specific time period
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="endpoint">Optional endpoint filter</param>
        /// <param name="onlyErrors">Include only failed calls</param>
        /// <returns>Count of metrics in the specified period</returns>
        Task<int> CountInPeriodAsync(DateTime fromDate, DateTime toDate, string? endpoint = null, bool onlyErrors = false);

        /// <summary>
        /// Get paged metrics in a specific time period
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="endpoint">Optional endpoint filter</param>
        /// <param name="onlyErrors">Include only failed calls</param>
        /// <param name="skipCount">Number of records to skip</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <param name="sorting">Sorting specification</param>
        /// <returns>Paged metrics in the specified period</returns>
        Task<List<ApiCallMetric>> GetPagedInPeriodAsync(
            DateTime fromDate, 
            DateTime toDate, 
            string? endpoint = null, 
            bool onlyErrors = false, 
            int skipCount = 0, 
            int maxResultCount = 50, 
            string? sorting = null);

        /// <summary>
        /// Get metrics for a specific operation
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Metrics for the specified operation</returns>
        Task<List<ApiCallMetric>> GetByOperationAsync(string operation, DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get metrics for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Metrics for the specified user</returns>
        Task<List<ApiCallMetric>> GetByUserIdAsync(Guid userId, DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get failed API calls
        /// </summary>
        /// <param name="since">Date to get failures since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Failed API calls</returns>
        Task<List<ApiCallMetric>> GetFailuresAsync(DateTime? since = null, int maxResultCount = 100);

        /// <summary>
        /// Get API call statistics
        /// </summary>
        /// <param name="since">Date to calculate statistics since</param>
        /// <param name="operation">Specific operation to get stats for (optional)</param>
        /// <returns>API call statistics</returns>
        Task<ApiCallStatistics> GetStatisticsAsync(DateTime? since = null, string? operation = null);

        /// <summary>
        /// Get metrics grouped by time period
        /// </summary>
        /// <param name="since">Start date</param>
        /// <param name="until">End date</param>
        /// <param name="groupBy">Grouping period (hour, day, week)</param>
        /// <returns>Metrics grouped by time period</returns>
        Task<List<TimeGroupedMetrics>> GetMetricsByTimePeriodAsync(DateTime since, DateTime until, TimeGrouping groupBy);

        /// <summary>
        /// Get slow API calls (above a certain duration threshold)
        /// </summary>
        /// <param name="thresholdMs">Duration threshold in milliseconds</param>
        /// <param name="since">Date to get metrics since</param>
        /// <param name="maxResultCount">Maximum number of records to return</param>
        /// <returns>Slow API calls</returns>
        Task<List<ApiCallMetric>> GetSlowCallsAsync(long thresholdMs, DateTime? since = null, int maxResultCount = 50);

        /// <summary>
        /// Clean up old metrics
        /// </summary>
        /// <param name="olderThan">Date threshold for cleanup</param>
        /// <returns>Number of records deleted</returns>
        Task<int> CleanupOldMetricsAsync(DateTime olderThan);

        /// <summary>
        /// Get metrics summary for the last N days
        /// </summary>
        /// <param name="days">Number of days to include</param>
        /// <returns>Daily metrics summary</returns>
        Task<List<DailyMetricsSummary>> GetDailySummaryAsync(int days = 7);

        /// <summary>
        /// Get endpoint performance metrics
        /// </summary>
        /// <param name="since">Date to get metrics since</param>
        /// <returns>Performance metrics by endpoint</returns>
        Task<List<EndpointPerformanceMetrics>> GetEndpointPerformanceAsync(DateTime? since = null);
    }

    /// <summary>
    /// Statistics for API calls
    /// </summary>
    public class ApiCallStatistics
    {
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double AverageDurationMs { get; set; }
        public long MinDurationMs { get; set; }
        public long MaxDurationMs { get; set; }
        public double SuccessRate { get; set; }
        public long TotalDataTransferred { get; set; }
        public DateTime? FirstCall { get; set; }
        public DateTime? LastCall { get; set; }
        
        public Dictionary<string, int> CallsByOperation { get; set; } = new();
        public Dictionary<int, int> CallsByHttpStatus { get; set; } = new();
    }

    /// <summary>
    /// Metrics grouped by time period
    /// </summary>
    public class TimeGroupedMetrics
    {
        public DateTime Period { get; set; }
        public int CallCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double AverageDuration { get; set; }
        public long TotalDataTransferred { get; set; }
    }

    /// <summary>
    /// Daily metrics summary
    /// </summary>
    public class DailyMetricsSummary
    {
        public DateTime Date { get; set; }
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double AverageDuration { get; set; }
        public long TotalDataTransferred { get; set; }
        public string MostUsedOperation { get; set; } = string.Empty;
        public int UniqueUsers { get; set; }
    }

    /// <summary>
    /// Performance metrics by endpoint
    /// </summary>
    public class EndpointPerformanceMetrics
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public double AverageDuration { get; set; }
        public long MinDuration { get; set; }
        public long MaxDuration { get; set; }
        public double SuccessRate { get; set; }
        public long TotalDataTransferred { get; set; }
    }

    /// <summary>
    /// Time grouping options
    /// </summary>
    public enum TimeGrouping
    {
        Hour,
        Day,
        Week,
        Month
    }
}
