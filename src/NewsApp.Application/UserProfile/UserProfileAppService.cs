using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NewsApp.Domain.UserProfile;
using NewsApp.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace NewsApp.UserProfile
{
    /// <summary>
    /// Application service for user profile management
    /// </summary>
    [Authorize]
    public class UserProfileAppService : ApplicationService, IUserProfileAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IUserPreferencesRepository _userPreferencesRepository;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;

        public UserProfileAppService(
            IIdentityUserRepository userRepository,
            IdentityUserManager userManager,
            IUserPreferencesRepository userPreferencesRepository,
            IPasswordHasher<IdentityUser> passwordHasher)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _userPreferencesRepository = userPreferencesRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserProfileDto> GetMyProfileAsync()
        {
            var currentUserId = CurrentUser.GetId();
            var user = await _userRepository.GetAsync(currentUserId);
            var preferences = await _userPreferencesRepository.FindByUserIdAsync(currentUserId);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                NewsLanguageCode = preferences?.NewsLanguageCode ?? "en",
                NewsLanguageName = preferences?.NewsLanguageName ?? "English",
                CreationTime = user.CreationTime,
                LastModificationTime = user.LastModificationTime
            };
        }

        public async Task<ProfileUpdateResultDto> UpdateMyProfileAsync(UpdateUserProfileDto input)
        {
            try
            {
                var currentUserId = CurrentUser.GetId();
                var user = await _userRepository.GetAsync(currentUserId);

                // Check if username is already taken by another user
                if (user.UserName != input.UserName)
                {
                    var existingUser = await _userRepository.FindByNormalizedUserNameAsync(
                        _userManager.NormalizeName(input.UserName));
                    
                    if (existingUser != null && existingUser.Id != currentUserId)
                    {
                        return new ProfileUpdateResultDto
                        {
                            Success = false,
                            Message = L["UserNameAlreadyExists"]
                        };
                    }
                }

                // Check if email is already taken by another user
                var emailChanged = user.Email != input.Email;
                if (emailChanged)
                {
                    var existingUserByEmail = await _userRepository.FindByNormalizedEmailAsync(
                        _userManager.NormalizeEmail(input.Email));
                    
                    if (existingUserByEmail != null && existingUserByEmail.Id != currentUserId)
                    {
                        return new ProfileUpdateResultDto
                        {
                            Success = false,
                            Message = L["EmailAlreadyExists"]
                        };
                    }
                }

                // Update user properties
                await _userManager.SetUserNameAsync(user, input.UserName);
                await _userManager.SetEmailAsync(user, input.Email);
                
                user.Name = input.Name;
                user.Surname = input.Surname;
                await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);

                // If email changed, mark as unconfirmed
                if (emailChanged)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // Reset email confirmation status
                    user.SetEmailConfirmed(false);
                }

                await _userRepository.UpdateAsync(user);

                // Update or create user preferences
                var preferences = await _userPreferencesRepository.GetOrCreateByUserIdAsync(currentUserId);
                var availableLanguages = await GetAvailableNewsLanguagesAsync();
                var selectedLanguage = availableLanguages.FirstOrDefault(l => l.Code == input.NewsLanguageCode);
                
                if (selectedLanguage != null)
                {
                    preferences.SetNewsLanguage(selectedLanguage.Code, selectedLanguage.Name);
                    await _userPreferencesRepository.UpdateAsync(preferences);
                }

                var updatedProfile = await GetMyProfileAsync();

                return new ProfileUpdateResultDto
                {
                    Success = true,
                    Message = L["ProfileUpdatedSuccessfully"],
                    RequiresEmailConfirmation = emailChanged,
                    Profile = updatedProfile
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update user profile for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = L["ProfileUpdateFailed"]
                };
            }
        }

        public async Task<ProfileUpdateResultDto> ChangePasswordAsync(ChangePasswordDto input)
        {
            try
            {
                var currentUserId = CurrentUser.GetId();
                var user = await _userRepository.GetAsync(currentUserId);

                // Verify current password
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                    user, user.PasswordHash, input.CurrentPassword);

                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = L["CurrentPasswordIncorrect"]
                    };
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
                
                if (result.Succeeded)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = true,
                        Message = L["PasswordChangedSuccessfully"]
                    };
                }

                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to change password for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = L["PasswordChangeFailed"]
                };
            }
        }

        public Task<List<NewsLanguageDto>> GetAvailableNewsLanguagesAsync()
        {
            // Return supported languages based on your NewsApp domain configuration
            var languages = new List<NewsLanguageDto>
            {
                new() { Code = "de", Name = "Deutsch", IsSupported = true },
                new() { Code = "en", Name = "English", IsSupported = true },
                new() { Code = "es", Name = "Español", IsSupported = true },
                new() { Code = "fr", Name = "Français", IsSupported = true },
                new() { Code = "he", Name = "Hebrew", IsSupported = true },
                new() { Code = "it", Name = "Italiano", IsSupported = true },
                new() { Code = "nl", Name = "Nederlands", IsSupported = true },
                new() { Code = "no", Name = "Norsk", IsSupported = true },
                new() { Code = "pt", Name = "Português", IsSupported = true },
                new() { Code = "sv", Name = "Svenska", IsSupported = true }
            };
            
            return Task.FromResult(languages);
        }

        public async Task<ProfileUpdateResultDto> UpdateNewsLanguageAsync(string languageCode)
        {
            try
            {
                var currentUserId = CurrentUser.GetId();
                var availableLanguages = await GetAvailableNewsLanguagesAsync();
                var selectedLanguage = availableLanguages.FirstOrDefault(l => l.Code == languageCode);

                if (selectedLanguage == null)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = L["LanguageNotSupported"]
                    };
                }

                var preferences = await _userPreferencesRepository.GetOrCreateByUserIdAsync(currentUserId);
                preferences.SetNewsLanguage(selectedLanguage.Code, selectedLanguage.Name);
                await _userPreferencesRepository.UpdateAsync(preferences);

                return new ProfileUpdateResultDto
                {
                    Success = true,
                    Message = L["NewsLanguageUpdatedSuccessfully"]
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update news language for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = L["NewsLanguageUpdateFailed"]
                };
            }
        }

        public async Task<ProfileUpdateResultDto> SendEmailConfirmationAsync()
        {
            try
            {
                var currentUserId = CurrentUser.GetId();
                var user = await _userRepository.GetAsync(currentUserId);

                if (user.EmailConfirmed)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = L["EmailAlreadyConfirmed"]
                    };
                }

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // In a real application, you would send an email with this token
                // For now, we'll just return success
                Logger.LogInformation("Email confirmation token generated for user {UserId}: {Token}", user.Id, token);

                return new ProfileUpdateResultDto
                {
                    Success = true,
                    Message = L["EmailConfirmationSent"]
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send email confirmation for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = L["EmailConfirmationFailed"]
                };
            }
        }

        public async Task<ProfileUpdateResultDto> ConfirmEmailAsync(string token)
        {
            try
            {
                var currentUserId = CurrentUser.GetId();
                var user = await _userRepository.GetAsync(currentUserId);

                var result = await _userManager.ConfirmEmailAsync(user, token);
                
                if (result.Succeeded)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = true,
                        Message = L["EmailConfirmedSuccessfully"]
                    };
                }

                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to confirm email for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = L["EmailConfirmationFailed"]
                };
            }
        }
    }
}