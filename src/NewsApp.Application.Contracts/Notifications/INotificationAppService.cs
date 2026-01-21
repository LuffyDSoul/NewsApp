using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewsApp.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        Task<NotificationPreferenceDto> GetPreferencesAsync();
        Task<NotificationPreferenceDto> UpdatePreferencesAsync(NotificationPreferenceDto input);
        Task SendTestNotificationAsync();
    }
}
