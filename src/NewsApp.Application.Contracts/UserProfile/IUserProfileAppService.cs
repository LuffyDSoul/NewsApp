using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace NewsApp.UserProfile
{
    /// <summary>
    /// Application service interface for user profile management
    /// </summary>
    public interface IUserProfileAppService : IApplicationService
    {
        /// <summary>
        /// Get current user's profile information
        /// </summary>
        /// <returns>User profile data</returns>
        Task<UserProfileDto> GetMyProfileAsync();

        /// <summary>
        /// Update current user's profile information
        /// </summary>
        /// <param name="input">Profile update data</param>
        /// <returns>Update result</returns>
        Task<ProfileUpdateResultDto> UpdateMyProfileAsync(UpdateUserProfileDto input);

        /// <summary>
        /// Change current user's password
        /// </summary>
        /// <param name="input">Password change data</param>
        /// <returns>Update result</returns>
        Task<ProfileUpdateResultDto> ChangePasswordAsync(ChangePasswordDto input);

        /// <summary>
        /// Get available languages for news
        /// </summary>
        /// <returns>List of supported languages</returns>
        Task<List<NewsLanguageDto>> GetAvailableNewsLanguagesAsync();

        /// <summary>
        /// Update user's preferred news language
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "es")</param>
        /// <returns>Update result</returns>
        Task<ProfileUpdateResultDto> UpdateNewsLanguageAsync(string languageCode);

        /// <summary>
        /// Send email confirmation to current user
        /// </summary>
        /// <returns>Operation result</returns>
        Task<ProfileUpdateResultDto> SendEmailConfirmationAsync();

        /// <summary>
        /// Confirm email with token
        /// </summary>
        /// <param name="token">Email confirmation token</param>
        /// <returns>Operation result</returns>
        Task<ProfileUpdateResultDto> ConfirmEmailAsync(string token);
    }
}