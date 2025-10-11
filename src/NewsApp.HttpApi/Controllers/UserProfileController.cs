using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApp.UserProfile;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers
{
    /// <summary>
    /// Controller for user profile management operations
    /// </summary>
    [RemoteService]
    [Area("app")]
    [Route("api/app/user-profile")]
    [Authorize]
    public class UserProfileController : AbpControllerBase
    {
        private readonly IUserProfileAppService _userProfileAppService;

        public UserProfileController(IUserProfileAppService userProfileAppService)
        {
            _userProfileAppService = userProfileAppService;
        }

        /// <summary>
        /// Get current user's profile information
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet]
        public async Task<UserProfileDto> GetMyProfileAsync()
        {
            return await _userProfileAppService.GetMyProfileAsync();
        }

        /// <summary>
        /// Update current user's profile information
        /// </summary>
        /// <param name="input">Profile update data</param>
        /// <returns>Update result</returns>
        [HttpPut]
        public async Task<ProfileUpdateResultDto> UpdateMyProfileAsync(UpdateUserProfileDto input)
        {
            return await _userProfileAppService.UpdateMyProfileAsync(input);
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        /// <param name="input">Password change data</param>
        /// <returns>Update result</returns>
        [HttpPost("change-password")]
        public async Task<ProfileUpdateResultDto> ChangePasswordAsync(ChangePasswordDto input)
        {
            return await _userProfileAppService.ChangePasswordAsync(input);
        }

        /// <summary>
        /// Get available languages for news
        /// </summary>
        /// <returns>List of supported languages</returns>
        [HttpGet("news-languages")]
        public async Task<List<NewsLanguageDto>> GetAvailableNewsLanguagesAsync()
        {
            return await _userProfileAppService.GetAvailableNewsLanguagesAsync();
        }

        /// <summary>
        /// Update user's preferred news language
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "es")</param>
        /// <returns>Update result</returns>
        [HttpPost("news-language")]
        public async Task<ProfileUpdateResultDto> UpdateNewsLanguageAsync([FromBody] string languageCode)
        {
            return await _userProfileAppService.UpdateNewsLanguageAsync(languageCode);
        }

        /// <summary>
        /// Send email confirmation to current user
        /// </summary>
        /// <returns>Operation result</returns>
        [HttpPost("send-email-confirmation")]
        public async Task<ProfileUpdateResultDto> SendEmailConfirmationAsync()
        {
            return await _userProfileAppService.SendEmailConfirmationAsync();
        }

        /// <summary>
        /// Confirm email with token
        /// </summary>
        /// <param name="token">Email confirmation token</param>
        /// <returns>Operation result</returns>
        [HttpPost("confirm-email")]
        public async Task<ProfileUpdateResultDto> ConfirmEmailAsync([FromBody] string token)
        {
            return await _userProfileAppService.ConfirmEmailAsync(token);
        }
    }
}