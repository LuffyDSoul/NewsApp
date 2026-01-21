using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsApp.Email;
using NewsApp.News;
using NewsApp.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace NewsApp.Notifications
{
    public class NewsNotificationService : ITransientDependency
    {
        private readonly IRepository<Theme, int> _themeRepository;
        private readonly INewsService _newsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NewsNotificationService> _logger;
        private readonly NewsNotificationSettings _settings;
        private readonly Dictionary<string, DateTime> _lastCheckTimes;

        public NewsNotificationService(
            IRepository<Theme, int> themeRepository,
            INewsService newsService,
            IEmailService emailService,
            ILogger<NewsNotificationService> logger,
            IOptions<NewsNotificationSettings> settings)
        {
            _themeRepository = themeRepository;
            _newsService = newsService;
            _emailService = emailService;
            _logger = logger;
            _settings = settings.Value;
            _lastCheckTimes = new Dictionary<string, DateTime>();
        }

        public async Task CheckAndSendNotificationsAsync()
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("News notifications are disabled");
                return;
            }

            try
            {
                _logger.LogInformation("Starting news notification check...");

                // Obtener todos los temas con sus usuarios
                var themes = await _themeRepository.GetListAsync(includeDetails: true);

                foreach (var theme in themes.Where(t => t.User != null))
                {
                    try
                    {
                        await ProcessThemeNotificationAsync(theme);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing notifications for theme {ThemeId} - {ThemeName}", 
                            theme.Id, theme.Name);
                    }
                }

                _logger.LogInformation("News notification check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during news notification check");
            }
        }

        private async Task ProcessThemeNotificationAsync(Theme theme)
        {
            var cacheKey = $"{theme.User.Id}_{theme.Name}";
            
            // Verificar si ya se verificó recientemente (evitar duplicados)
            if (_lastCheckTimes.TryGetValue(cacheKey, out var lastCheck))
            {
                if ((DateTime.UtcNow - lastCheck).TotalMinutes < _settings.CheckIntervalMinutes)
                {
                    _logger.LogDebug("Skipping theme {ThemeName} - checked recently", theme.Name);
                    return;
                }
            }

            _logger.LogInformation("Checking news for theme: {ThemeName} (User: {UserEmail})", 
                theme.Name, theme.User.Email);

            // Buscar noticias relacionadas con el tema
            var articles = await _newsService.GetNewsAsync(theme.Name);

            if (articles != null && articles.Any())
            {
                // Filtrar artículos recientes (últimas horas según configuración)
                var recentArticles = articles
                    .Where(a => a.PublishedAt.HasValue && 
                               (DateTime.UtcNow - a.PublishedAt.Value.ToUniversalTime()).TotalMinutes 
                               <= _settings.CheckIntervalMinutes)
                    .ToList();

                if (recentArticles.Any())
                {
                    _logger.LogInformation("Found {Count} new articles for theme {ThemeName}", 
                        recentArticles.Count, theme.Name);

                    // Enviar notificación por email
                    await _emailService.SendNewsNotificationAsync(
                        theme.User.Email,
                        theme.User.UserName ?? theme.User.Email,
                        theme.Name,
                        recentArticles);

                    _logger.LogInformation("Notification sent to {Email} for theme {ThemeName}", 
                        theme.User.Email, theme.Name);
                }
                else
                {
                    _logger.LogDebug("No recent articles found for theme {ThemeName}", theme.Name);
                }
            }

            // Actualizar el timestamp de última verificación
            _lastCheckTimes[cacheKey] = DateTime.UtcNow;
        }

        public async Task SendDailySummaryAsync()
        {
            if (!_settings.Enabled || !_settings.SendDailySummary)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Starting daily summary generation...");

                var themes = await _themeRepository.GetListAsync(includeDetails: true);
                var userThemes = themes
                    .Where(t => t.User != null)
                    .GroupBy(t => t.User.Id);

                foreach (var userGroup in userThemes)
                {
                    try
                    {
                        await SendUserDailySummaryAsync(userGroup);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending daily summary for user {UserId}", userGroup.Key);
                    }
                }

                _logger.LogInformation("Daily summary generation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during daily summary generation");
            }
        }

        private async Task SendUserDailySummaryAsync(IGrouping<Guid, Theme> userThemes)
        {
            var user = userThemes.First().User;
            var allArticles = new List<News.ArticleDto>();

            foreach (var theme in userThemes)
            {
                try
                {
                    var articles = await _newsService.GetNewsAsync(theme.Name);
                    if (articles != null && articles.Any())
                    {
                        var recentArticles = articles
                            .Where(a => a.PublishedAt.HasValue && 
                                       (DateTime.UtcNow - a.PublishedAt.Value.ToUniversalTime()).TotalHours <= 24)
                            .ToList();

                        allArticles.AddRange(recentArticles);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching news for theme {ThemeName}", theme.Name);
                }
            }

            if (allArticles.Any())
            {
                // Eliminar duplicados por URL
                var uniqueArticles = allArticles
                    .GroupBy(a => a.Url)
                    .Select(g => g.First())
                    .OrderByDescending(a => a.PublishedAt)
                    .ToList();

                await _emailService.SendNewsNotificationAsync(
                    user.Email,
                    user.UserName ?? user.Email,
                    "Resumen Diario de Noticias",
                    uniqueArticles);

                _logger.LogInformation("Daily summary sent to {Email} with {Count} articles", 
                    user.Email, uniqueArticles.Count);
            }
        }
    }
}
