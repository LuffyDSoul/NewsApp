using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsApp.NewsAlerts;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers
{
    [Area("app")]
    [Route("api/news-alerts")]
    public class NewsAlertController : AbpControllerBase
    {
        private readonly INewsAlertAppService _newsAlertAppService;

        public NewsAlertController(INewsAlertAppService newsAlertAppService)
        {
            _newsAlertAppService = newsAlertAppService;
        }

        /// <summary>
        /// Get all alert lists for the current user
        /// </summary>
        [HttpGet]
        public async Task<List<NewsAlertListDto>> GetMyAlertsAsync([FromQuery] bool? isActive = null)
        {
            return await _newsAlertAppService.GetMyAlertsAsync(isActive);
        }

        /// <summary>
        /// Get a specific alert list by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<NewsAlertListDto> GetAsync(Guid id)
        {
            return await _newsAlertAppService.GetAsync(id);
        }

        /// <summary>
        /// Create a new alert list
        /// </summary>
        [HttpPost]
        public async Task<NewsAlertListDto> CreateAsync([FromBody] CreateNewsAlertListDto input)
        {
            return await _newsAlertAppService.CreateAsync(input);
        }

        /// <summary>
        /// Update an existing alert list
        /// </summary>
        [HttpPut("{id}")]
        public async Task<NewsAlertListDto> UpdateAsync(Guid id, [FromBody] UpdateNewsAlertListDto input)
        {
            return await _newsAlertAppService.UpdateAsync(id, input);
        }

        /// <summary>
        /// Delete an alert list
        /// </summary>
        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _newsAlertAppService.DeleteAsync(id);
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        [HttpGet("notifications")]
        public async Task<List<NewsAlertNotificationDto>> GetMyNotificationsAsync(
            [FromQuery] bool? unreadOnly = null,
            [FromQuery] int maxCount = 50)
        {
            return await _newsAlertAppService.GetMyNotificationsAsync(unreadOnly, maxCount);
        }

        /// <summary>
        /// Get count of unread notifications
        /// </summary>
        [HttpGet("notifications/unread-count")]
        public async Task<int> GetUnreadNotificationsCountAsync()
        {
            return await _newsAlertAppService.GetUnreadNotificationsCountAsync();
        }

        /// <summary>
        /// Mark a specific notification as read
        /// </summary>
        [HttpPut("notifications/{id}/read")]
        public async Task MarkNotificationAsReadAsync(Guid id)
        {
            await _newsAlertAppService.MarkNotificationAsReadAsync(id);
        }

        /// <summary>
        /// Mark all notifications as read for the current user
        /// </summary>
        [HttpPut("notifications/read-all")]
        public async Task MarkAllNotificationsAsReadAsync()
        {
            await _newsAlertAppService.MarkAllNotificationsAsReadAsync();
        }

        /// <summary>
        /// Trigger manual check of alerts (for testing)
        /// </summary>
        [HttpPost("check-now")]
        public async Task<IActionResult> TriggerAlertCheckAsync()
        {
            var result = await _newsAlertAppService.TriggerManualCheckAsync();
            return Ok(new { message = result });
        }
    }
}
