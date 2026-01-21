using Microsoft.AspNetCore.Mvc;
using NewsApp.Notifications;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers
{
    [Area("app")]
    [Route("api/notifications")]
    public class NotificationController : AbpControllerBase
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationController(INotificationAppService notificationAppService)
        {
            _notificationAppService = notificationAppService;
        }

        /// <summary>
        /// Obtener las preferencias de notificación del usuario actual
        /// </summary>
        [HttpGet("preferences")]
        public async Task<NotificationPreferenceDto> GetPreferencesAsync()
        {
            return await _notificationAppService.GetPreferencesAsync();
        }

        /// <summary>
        /// Actualizar las preferencias de notificación del usuario actual
        /// </summary>
        [HttpPut("preferences")]
        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync([FromBody] NotificationPreferenceDto input)
        {
            return await _notificationAppService.UpdatePreferencesAsync(input);
        }

        /// <summary>
        /// Enviar una notificación de prueba al usuario actual
        /// </summary>
        [HttpPost("test")]
        public async Task SendTestNotificationAsync()
        {
            await _notificationAppService.SendTestNotificationAsync();
        }
    }
}
