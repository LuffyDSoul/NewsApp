namespace NewsApp.Permissions;

/// <summary>
/// Permission names for the NewsApp application
/// </summary>
public static class NewsAppPermissions
{
    public const string GroupName = "NewsApp";

    /// <summary>
    /// News-related permissions
    /// </summary>
    public static class News
    {
        public const string Default = GroupName + ".News";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Search = Default + ".Search";
        public const string ViewSources = Default + ".ViewSources";
    }

    /// <summary>
    /// Reading list permissions
    /// </summary>
    public static class Lists
    {
        public const string Default = GroupName + ".Lists";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Manage = Default + ".Manage";
        public const string ViewPublic = Default + ".ViewPublic";
        public const string Share = Default + ".Share";
    }

    /// <summary>
    /// Alert permissions
    /// </summary>
    public static class Alerts
    {
        public const string Default = GroupName + ".Alerts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Manage = Default + ".Manage";
        public const string Trigger = Default + ".Trigger";
        public const string ViewResults = Default + ".ViewResults";
        public const string Execute = Default + ".Execute";
        public const string Import = Default + ".Import";
        public const string Export = Default + ".Export";
    }

    /// <summary>
    /// Monitoring permissions
    /// </summary>
    public static class Monitoring
    {
        public const string Default = GroupName + ".Monitoring";
        public const string View = Default + ".View";
        public const string ViewMetrics = Default + ".ViewMetrics";
        public const string ViewDashboard = Default + ".ViewDashboard";
        public const string ViewDetailedMetrics = Default + ".ViewDetailedMetrics";
        public const string ManageMetrics = Default + ".ManageMetrics";
        public const string Export = Default + ".Export";
        public const string SystemHealth = Default + ".SystemHealth";
    }

    /// <summary>
    /// User profile permissions
    /// </summary>
    public static class Profile
    {
        public const string Default = GroupName + ".Profile";
        public const string View = Default + ".View";
        public const string Edit = Default + ".Edit";
        public const string ChangePassword = Default + ".ChangePassword";
        public const string ManageLanguage = Default + ".ManageLanguage";
        public const string ManageNotifications = Default + ".ManageNotifications";
    }

    /// <summary>
    /// User settings permissions
    /// </summary>
    public static class Settings
    {
        public const string Default = GroupName + ".Settings";
        public const string ManageProfile = Default + ".ManageProfile";
        public const string ManageNotifications = Default + ".ManageNotifications";
        public const string ManageLanguage = Default + ".ManageLanguage";
    }

    /// <summary>
    /// Administration permissions
    /// </summary>
    public static class Administration
    {
        public const string Default = GroupName + ".Administration";
        public const string ManageUsers = Default + ".ManageUsers";
        public const string ViewAllData = Default + ".ViewAllData";
        public const string SystemConfiguration = Default + ".SystemConfiguration";
        public const string CleanupData = Default + ".CleanupData";
    }
}
