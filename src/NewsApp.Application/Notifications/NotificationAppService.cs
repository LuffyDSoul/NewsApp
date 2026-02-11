using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NewsApp.Email;
using NewsApp.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace NewsApp.Notifications
{
    [Authorize]
    public class NotificationAppService : NewsAppAppService, INotificationAppService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly INewsService _newsService;
        private readonly ILogger<NotificationAppService> _logger;

        public NotificationAppService(
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            INewsService newsService,
            ILogger<NotificationAppService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _newsService = newsService;
            _logger = logger;
        }

        public async Task<NotificationPreferenceDto> GetPreferencesAsync()
        {
            // Por ahora retornamos preferencias por defecto
            // En una implementaci�n completa, estas se guardar�an en la base de datos
            return await Task.FromResult(new NotificationPreferenceDto
            {
                EnableNotifications = true,
                EnableDailySummary = true,
                PreferredSummaryHour = 9
            });
        }

        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(NotificationPreferenceDto input)
        {
            // Por ahora solo validamos y retornamos
            // En una implementaci�n completa, estas se guardar�an en la base de datos
            if (input.PreferredSummaryHour < 0 || input.PreferredSummaryHour > 23)
            {
                throw new ArgumentException("PreferredSummaryHour debe estar entre 0 y 23");
            }

            _logger.LogInformation("User {UserId} updated notification preferences", CurrentUser.Id);
            return await Task.FromResult(input);
        }

        public async Task SendTestNotificationAsync()
        {
            var userGuid = CurrentUser.Id.GetValueOrDefault();
            var user = await _userManager.FindByIdAsync(userGuid.ToString());

            if (user == null)
            {
                throw new InvalidOperationException("Usuario no encontrado");
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                throw new InvalidOperationException("No tienes una direcci�n de email configurada en tu perfil");
            }

            _logger.LogInformation("Sending test notification to user {UserId} at email {Email}", user.Id, user.Email);

            // Obtener algunas noticias de prueba
            var articles = await _newsService.GetNewsAsync("technology");
            var testArticles = articles.Take(3).ToList();

            await _emailService.SendNewsNotificationAsync(
                user.Email,
                user.UserName ?? user.Email,
                "Test - Tecnolog�a",
                testArticles);

            _logger.LogInformation("Test notification sent successfully to {Email}", user.Email);
        }
    }
}
