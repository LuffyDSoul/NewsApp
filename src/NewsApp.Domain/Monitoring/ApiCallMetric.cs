using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace NewsApp.Domain.Monitoring
{
    /// <summary>
    /// Represents metrics for API calls to external services (like NewsAPI)
    /// </summary>
    public class ApiCallMetric : Entity<Guid>
    {
        /// <summary>
        /// When the API call was made
        /// </summary>
        public DateTime When { get; set; }

        /// <summary>
        /// Operation that was performed (e.g., "SearchNews", "GetTopHeadlines")
        /// </summary>
        [Required]
        [MaxLength(100)]
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
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Endpoint that was called
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// User ID who initiated the call (if applicable)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Request size in bytes (if applicable)
        /// </summary>
        public long? RequestSizeBytes { get; set; }

        /// <summary>
        /// Response size in bytes (if applicable)
        /// </summary>
        public long? ResponseSizeBytes { get; set; }

        /// <summary>
        /// Additional metadata as JSON
        /// </summary>
        public string? Metadata { get; set; }

        protected ApiCallMetric()
        {
            // For EF Core
        }

        public ApiCallMetric(
            Guid id,
            string operation,
            string endpoint,
            long durationMs,
            int httpStatus,
            bool success,
            DateTime? when = null,
            string? errorMessage = null,
            Guid? userId = null) : base(id)
        {
            When = when ?? DateTime.UtcNow;
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            DurationMs = durationMs;
            HttpStatus = httpStatus;
            Success = success;
            ErrorMessage = errorMessage;
            UserId = userId;
        }

        public static ApiCallMetric CreateSuccess(
            string operation,
            string endpoint,
            long durationMs,
            int httpStatus,
            Guid? userId = null,
            long? requestSizeBytes = null,
            long? responseSizeBytes = null,
            string? metadata = null)
        {
            var metric = new ApiCallMetric(Guid.NewGuid(), operation, endpoint, durationMs, httpStatus, true, null, null, userId);
            metric.RequestSizeBytes = requestSizeBytes;
            metric.ResponseSizeBytes = responseSizeBytes;
            metric.Metadata = metadata;
            return metric;
        }

        public static ApiCallMetric CreateFailure(
            string operation,
            string endpoint,
            long durationMs,
            int httpStatus,
            string errorMessage,
            Guid? userId = null,
            string? metadata = null)
        {
            var metric = new ApiCallMetric(Guid.NewGuid(), operation, endpoint, durationMs, httpStatus, false, null, errorMessage, userId);
            metric.Metadata = metadata;
            return metric;
        }

        public void SetSizeMetrics(long? requestSizeBytes, long? responseSizeBytes)
        {
            RequestSizeBytes = requestSizeBytes;
            ResponseSizeBytes = responseSizeBytes;
        }

        public void SetMetadata(string? metadata)
        {
            Metadata = metadata;
        }
    }
}
