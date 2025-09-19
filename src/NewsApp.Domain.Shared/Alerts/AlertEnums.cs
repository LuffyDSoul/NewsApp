namespace NewsApp.Domain.Alerts
{
    /// <summary>
    /// Types of alerts that can be configured
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// Alert based on search query - triggers when new articles match the query
        /// </summary>
        SearchQuery = 1,

        /// <summary>
        /// Alert based on reading list - triggers when new articles are added to the list
        /// </summary>
        List = 2
    }

    /// <summary>
    /// Frequency options for alert execution
    /// </summary>
    public enum AlertFrequency
    {
        /// <summary>
        /// Run every 15 minutes
        /// </summary>
        EveryFifteenMinutes = 1,

        /// <summary>
        /// Run every hour
        /// </summary>
        Hourly = 2,

        /// <summary>
        /// Run every 6 hours
        /// </summary>
        SixHourly = 3,

        /// <summary>
        /// Run daily
        /// </summary>
        Daily = 4,

        /// <summary>
        /// Run weekly
        /// </summary>
        Weekly = 5
    }

    /// <summary>
    /// Frequency unit options for alert execution
    /// </summary>
    public enum AlertFrequencyUnit
    {
        /// <summary>
        /// Time unit in minutes
        /// </summary>
        Minutes = 1,

        /// <summary>
        /// Time unit in hours
        /// </summary>
        Hours = 2,

        /// <summary>
        /// Time unit in days
        /// </summary>
        Days = 3,

        /// <summary>
        /// Time unit in weeks
        /// </summary>
        Weeks = 4
    }

    /// <summary>
    /// Status of alert execution
    /// </summary>
    public enum AlertExecutionStatus
    {
        /// <summary>
        /// Execution was successful
        /// </summary>
        Success = 1,

        /// <summary>
        /// Execution failed
        /// </summary>
        Failed = 2,

        /// <summary>
        /// Execution is currently running
        /// </summary>
        Running = 3,

        /// <summary>
        /// Execution was cancelled
        /// </summary>
        Cancelled = 4
    }
}
