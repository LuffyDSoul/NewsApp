using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace NewsApp.Monitoring
{
    /// <summary>
    /// DTO for API call metrics
    /// </summary>
    public class ApiCallMetricDto : EntityDto<Guid>
    {
        /// <summary>
        /// When the API call was made
        /// </summary>
        public DateTime When { get; set; }

        /// <summary>
        /// Operation that was performed
        /// </summary>
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// Duration of the API call in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// HTTP status code returned by the API
        /// </summary>
        public int HttpStatus { get; set; }

        /// <summary>
        /// Whether the API call was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the call failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Endpoint that was called
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// User ID who initiated the call
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Request size in bytes
        /// </summary>
        public long? RequestSizeBytes { get; set; }

        /// <summary>
        /// Response size in bytes
        /// </summary>
        public long? ResponseSizeBytes { get; set; }

        /// <summary>
        /// Call time (mapped from When for consistency)
        /// </summary>
        public DateTime CallTime => When;

        /// <summary>
        /// Response time in milliseconds (mapped from DurationMs for consistency)
        /// </summary>
        public long ResponseTimeMs => DurationMs;

        /// <summary>
        /// Is success (mapped from Success for consistency)
        /// </summary>
        public bool IsSuccess => Success;

        /// <summary>
        /// Status code (mapped from HttpStatus for consistency)
        /// </summary>
        public int StatusCode => HttpStatus;

        /// <summary>
        /// Data size in bytes (mapped from ResponseSizeBytes for consistency)
        /// </summary>
        public long DataSizeBytes => ResponseSizeBytes ?? 0;
    }

    /// <summary>
    /// DTO for API call statistics
    /// </summary>
    public class ApiCallStatisticsDto
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
        
        public OperationMetricsDto[] CallsByOperation { get; set; } = Array.Empty<OperationMetricsDto>();
        public HttpStatusMetricsDto[] CallsByHttpStatus { get; set; } = Array.Empty<HttpStatusMetricsDto>();
    }

    /// <summary>
    /// DTO for operation-specific metrics
    /// </summary>
    public class OperationMetricsDto
    {
        public string Operation { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public double AverageDuration { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// DTO for HTTP status metrics
    /// </summary>
    public class HttpStatusMetricsDto
    {
        public int HttpStatus { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO for time-grouped metrics
    /// </summary>
    public class TimeGroupedMetricsDto
    {
        public DateTime Period { get; set; }
        public int CallCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double AverageDuration { get; set; }
        public long TotalDataTransferred { get; set; }
    }

    /// <summary>
    /// DTO for daily metrics summary
    /// </summary>
    public class DailyMetricsSummaryDto
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
    /// DTO for endpoint performance metrics
    /// </summary>
    public class EndpointPerformanceMetricsDto
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
    /// DTO for monitoring dashboard
    /// </summary>
    public class MonitoringDashboardDto
    {
        public ApiCallStatisticsDto OverallStatistics { get; set; } = new();
        public DailyMetricsSummaryDto[] Last7Days { get; set; } = Array.Empty<DailyMetricsSummaryDto>();
        public EndpointPerformanceMetricsDto[] TopEndpoints { get; set; } = Array.Empty<EndpointPerformanceMetricsDto>();
        public ApiCallMetricDto[] RecentFailures { get; set; } = Array.Empty<ApiCallMetricDto>();
        public ApiCallMetricDto[] SlowestCalls { get; set; } = Array.Empty<ApiCallMetricDto>();
        public TimeGroupedMetricsDto[] HourlyMetrics { get; set; } = Array.Empty<TimeGroupedMetricsDto>();
    }

    /// <summary>
    /// DTO for monitoring filters
    /// </summary>
    public class MonitoringFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Operation { get; set; }
        public bool? SuccessOnly { get; set; }
        public bool? FailuresOnly { get; set; }
        public long? MinDurationMs { get; set; }
        public long? MaxDurationMs { get; set; }
        public Guid? UserId { get; set; }
    }

    /// <summary>
    /// DTO for API call history filter
    /// </summary>
    public class ApiCallHistoryFilterDto : MonitoringFilterDto
    {
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = true;
        public string? Endpoint { get; set; }
        public bool OnlyErrors { get; set; }
    }

    /// <summary>
    /// DTO for API usage statistics
    /// </summary>
    public class ApiUsageStatisticsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long TotalDataTransferred { get; set; }
        public List<DailyApiUsageDto> DailyStatistics { get; set; } = new();
        public List<EndpointUsageDto> EndpointStatistics { get; set; } = new();
        public List<ApiErrorStatsDto> ErrorStatistics { get; set; } = new();
    }

    /// <summary>
    /// DTO for daily API usage
    /// </summary>
    public class DailyApiUsageDto
    {
        public DateTime Date { get; set; }
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double AverageResponseTime { get; set; }
        public long DataTransferred { get; set; }
    }

    /// <summary>
    /// DTO for endpoint usage statistics
    /// </summary>
    public class EndpointUsageDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double AverageResponseTime { get; set; }
        public long DataTransferred { get; set; }
    }

    /// <summary>
    /// DTO for API error statistics
    /// </summary>
    public class ApiErrorStatsDto
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime FirstOccurrence { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    /// <summary>
    /// DTO for performance metrics
    /// </summary>
    public class PerformanceMetricsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public double MedianResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
        public int TotalRequests { get; set; }
        public double ErrorRate { get; set; }
        public double ThroughputPerHour { get; set; }
        public List<ResponseTimeDistributionDto> ResponseTimeDistribution { get; set; } = new();
        public bool NoDataAvailable { get; set; }
    }

    /// <summary>
    /// DTO for response time distribution
    /// </summary>
    public class ResponseTimeDistributionDto
    {
        public string Range { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO for dashboard statistics
    /// </summary>
    public class DashboardStatisticsDto
    {
        public DateTime GeneratedAt { get; set; }
        public NewsStatisticsDto NewsStatistics { get; set; } = new();
        public AlertStatisticsOverviewDto AlertStatistics { get; set; } = new();
        public ReadingListStatisticsDto ReadingListStatistics { get; set; } = new();
        public ApiStatisticsOverviewDto ApiStatistics { get; set; } = new();
        public QuickMetricsDto QuickMetrics { get; set; } = new();
    }

    /// <summary>
    /// DTO for news statistics
    /// </summary>
    public class NewsStatisticsDto
    {
        public int TotalArticles { get; set; }
        public int ArticlesLast24Hours { get; set; }
        public int ArticlesLast7Days { get; set; }
        public double ArticlesGrowthRate { get; set; }
    }

    /// <summary>
    /// DTO for alert statistics overview
    /// </summary>
    public class AlertStatisticsOverviewDto
    {
        public int TotalAlerts { get; set; }
        public int ActiveAlerts { get; set; }
        public int AlertExecutionsLast24Hours { get; set; }
        public double AlertSuccessRate { get; set; }
    }

    /// <summary>
    /// DTO for reading list statistics
    /// </summary>
    public class ReadingListStatisticsDto
    {
        public int TotalLists { get; set; }
        public int PublicLists { get; set; }
        public double AverageItemsPerList { get; set; }
        public string MostActiveList { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for API statistics overview
    /// </summary>
    public class ApiStatisticsOverviewDto
    {
        public int CallsLast24Hours { get; set; }
        public int CallsLast7Days { get; set; }
        public int ErrorsLast24Hours { get; set; }
        public double AverageResponseTime { get; set; }
        public double ErrorRate { get; set; }
    }

    /// <summary>
    /// DTO for quick metrics
    /// </summary>
    public class QuickMetricsDto
    {
        public string SystemHealth { get; set; } = string.Empty;
        public int ActiveUsers { get; set; }
        public string DataStorage { get; set; } = string.Empty;
        public DateTime? LastBackup { get; set; }
    }

    /// <summary>
    /// DTO for trend data
    /// </summary>
    public class TrendDataDto
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// DTO for alert trend
    /// </summary>
    public class AlertTrendDto
    {
        public DateTime Date { get; set; }
        public int TriggeredAlerts { get; set; }
        public int NewAlerts { get; set; }
        public int ActiveAlerts { get; set; }
    }

    /// <summary>
    /// DTO for export metrics
    /// </summary>
    public class ExportMetricsDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Format { get; set; } = "csv"; // csv, json, excel
        public bool IncludeDetails { get; set; } = true;
        public string[]? Operations { get; set; }
    }

    /// <summary>
    /// DTO for export metrics result
    /// </summary>
    public class ExportMetricsResultDto
    {
        public string Content { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int RecordCount { get; set; }
    }

    // Extended health DTOs specific to the service implementation
    /// <summary>
    /// Extended DTO for API health
    /// </summary>
    public class ApiHealthDto
    {
        public bool IsHealthy { get; set; }
        public DateTime? LastCallTime { get; set; }
        public double RecentErrorRate { get; set; }
        public double AverageResponseTime { get; set; }
        public int CallsInLastHour { get; set; }
        public int CallsInLast24Hours { get; set; }
    }

    /// <summary>
    /// Extended DTO for database health
    /// </summary>
    public class DatabaseHealthDto
    {
        public bool IsHealthy { get; set; }
        public double ConnectionTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Extended DTO for service health
    /// </summary>
    public class ServiceHealthDto
    {
        public bool IsHealthy { get; set; }
        public TimeSpan Uptime { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public int ActiveConnections { get; set; }
    }

    /// <summary>
    /// Extended system health DTO for the service implementation
    /// </summary>
    public class ExtendedSystemHealthDto
    {
        public bool IsHealthy { get; set; }
        public DateTime CheckTime { get; set; }
        public ApiHealthDto ApiHealth { get; set; } = new();
        public DatabaseHealthDto DatabaseHealth { get; set; } = new();
        public ServiceHealthDto ServiceHealth { get; set; } = new();
        public List<string> Issues { get; set; } = new();
    }
}
