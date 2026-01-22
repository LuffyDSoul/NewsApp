namespace NewsApp.Notifications
{
    public class NotificationPreferenceDto
    {
        public bool EnableNotifications { get; set; } = true;
        public bool EnableDailySummary { get; set; } = true;
        public int PreferredSummaryHour { get; set; } = 9;
    }
}
