using NewsApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace NewsApp.Permissions;

/// <summary>
/// Permission definition provider for NewsApp
/// </summary>
public class NewsAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var newsAppGroup = context.AddGroup(NewsAppPermissions.GroupName, L("Permission:NewsApp"));

        // News permissions
        var newsPermission = newsAppGroup.AddPermission(NewsAppPermissions.News.Default, L("Permission:News"));
        newsPermission.AddChild(NewsAppPermissions.News.Create, L("Permission:News.Create"));
        newsPermission.AddChild(NewsAppPermissions.News.Edit, L("Permission:News.Edit"));
        newsPermission.AddChild(NewsAppPermissions.News.Delete, L("Permission:News.Delete"));
        newsPermission.AddChild(NewsAppPermissions.News.Search, L("Permission:News.Search"));
        newsPermission.AddChild(NewsAppPermissions.News.ViewSources, L("Permission:News.ViewSources"));

        // Reading Lists permissions
        var listsPermission = newsAppGroup.AddPermission(NewsAppPermissions.Lists.Default, L("Permission:Lists"));
        listsPermission.AddChild(NewsAppPermissions.Lists.Create, L("Permission:Lists.Create"));
        listsPermission.AddChild(NewsAppPermissions.Lists.Edit, L("Permission:Lists.Edit"));
        listsPermission.AddChild(NewsAppPermissions.Lists.Delete, L("Permission:Lists.Delete"));
        listsPermission.AddChild(NewsAppPermissions.Lists.Manage, L("Permission:Lists.Manage"));
        listsPermission.AddChild(NewsAppPermissions.Lists.ViewPublic, L("Permission:Lists.ViewPublic"));
        listsPermission.AddChild(NewsAppPermissions.Lists.Share, L("Permission:Lists.Share"));

        // Alerts permissions
        var alertsPermission = newsAppGroup.AddPermission(NewsAppPermissions.Alerts.Default, L("Permission:Alerts"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.Create, L("Permission:Alerts.Create"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.Edit, L("Permission:Alerts.Edit"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.Delete, L("Permission:Alerts.Delete"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.Manage, L("Permission:Alerts.Manage"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.Trigger, L("Permission:Alerts.Trigger"));
        alertsPermission.AddChild(NewsAppPermissions.Alerts.ViewResults, L("Permission:Alerts.ViewResults"));

        // Monitoring permissions
        var monitoringPermission = newsAppGroup.AddPermission(NewsAppPermissions.Monitoring.Default, L("Permission:Monitoring"));
        monitoringPermission.AddChild(NewsAppPermissions.Monitoring.View, L("Permission:Monitoring.View"));
        monitoringPermission.AddChild(NewsAppPermissions.Monitoring.ViewDetailedMetrics, L("Permission:Monitoring.ViewDetailedMetrics"));
        monitoringPermission.AddChild(NewsAppPermissions.Monitoring.ManageMetrics, L("Permission:Monitoring.ManageMetrics"));
        monitoringPermission.AddChild(NewsAppPermissions.Monitoring.Export, L("Permission:Monitoring.Export"));
        monitoringPermission.AddChild(NewsAppPermissions.Monitoring.SystemHealth, L("Permission:Monitoring.SystemHealth"));

        // User Profile permissions
        var profilePermission = newsAppGroup.AddPermission(NewsAppPermissions.Profile.Default, L("Permission:Profile"));
        profilePermission.AddChild(NewsAppPermissions.Profile.View, L("Permission:Profile.View"));
        profilePermission.AddChild(NewsAppPermissions.Profile.Edit, L("Permission:Profile.Edit"));
        profilePermission.AddChild(NewsAppPermissions.Profile.ChangePassword, L("Permission:Profile.ChangePassword"));
        profilePermission.AddChild(NewsAppPermissions.Profile.ManageLanguage, L("Permission:Profile.ManageLanguage"));
        profilePermission.AddChild(NewsAppPermissions.Profile.ManageNotifications, L("Permission:Profile.ManageNotifications"));

        // Settings permissions
        var settingsPermission = newsAppGroup.AddPermission(NewsAppPermissions.Settings.Default, L("Permission:Settings"));
        settingsPermission.AddChild(NewsAppPermissions.Settings.ManageProfile, L("Permission:Settings.ManageProfile"));
        settingsPermission.AddChild(NewsAppPermissions.Settings.ManageNotifications, L("Permission:Settings.ManageNotifications"));
        settingsPermission.AddChild(NewsAppPermissions.Settings.ManageLanguage, L("Permission:Settings.ManageLanguage"));

        // Administration permissions
        var adminPermission = newsAppGroup.AddPermission(NewsAppPermissions.Administration.Default, L("Permission:Administration"));
        adminPermission.AddChild(NewsAppPermissions.Administration.ManageUsers, L("Permission:Administration.ManageUsers"));
        adminPermission.AddChild(NewsAppPermissions.Administration.ViewAllData, L("Permission:Administration.ViewAllData"));
        adminPermission.AddChild(NewsAppPermissions.Administration.SystemConfiguration, L("Permission:Administration.SystemConfiguration"));
        adminPermission.AddChild(NewsAppPermissions.Administration.CleanupData, L("Permission:Administration.CleanupData"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<NewsAppResource>(name);
    }
}
