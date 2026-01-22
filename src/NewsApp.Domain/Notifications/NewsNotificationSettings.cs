using System;

namespace NewsApp.Notifications
{
    public class NewsNotificationSettings
    {
        public bool Enabled { get; set; } = true;
        public int CheckIntervalMinutes { get; set; } = 60;
        public bool SendDailySummary { get; set; } = true;
        public int DailySummaryHour { get; set; } = 9;
    }
}
