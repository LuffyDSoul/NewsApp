using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace NewsApp.UserProfile
{
    /// <summary>
    /// DTO for user profile information
    /// </summary>
    public class UserProfileDto : EntityDto<Guid>
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's username
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        [MaxLength(64)]
        public string? Name { get; set; }

        /// <summary>
        /// User's surname
        /// </summary>
        [MaxLength(64)]
        public string? Surname { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [MaxLength(16)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Whether the email is confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Whether the phone number is confirmed
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// User's preferred language code for news
        /// </summary>
        [MaxLength(10)]
        public string NewsLanguageCode { get; set; } = "en";

        /// <summary>
        /// User's preferred language name for news
        /// </summary>
        [MaxLength(50)]
        public string NewsLanguageName { get; set; } = "English";

        /// <summary>
        /// Whether two-factor authentication is enabled
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// User creation time
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Last modification time
        /// </summary>
        public DateTime? LastModificationTime { get; set; }
    }

    /// <summary>
    /// DTO for updating user profile
    /// </summary>
    public class UpdateUserProfileDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User's username
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        [MaxLength(64)]
        public string? Name { get; set; }

        /// <summary>
        /// User's surname
        /// </summary>
        [MaxLength(64)]
        public string? Surname { get; set; }

        /// <summary>
        /// User's phone number
        /// </summary>
        [MaxLength(16)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User's preferred language code for news
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string NewsLanguageCode { get; set; } = "en";
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// Current password
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(128)]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of new password
        /// </summary>
        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for available news languages
    /// </summary>
    public class NewsLanguageDto
    {
        /// <summary>
        /// Language code (e.g., "en", "es", "fr")
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Language display name (e.g., "English", "Español", "Français")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether this language is supported by the news API
        /// </summary>
        public bool IsSupported { get; set; } = true;
    }

    /// <summary>
    /// Result DTO for profile update operations
    /// </summary>
    public class ProfileUpdateResultDto
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message about the operation result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Whether email confirmation is required
        /// </summary>
        public bool RequiresEmailConfirmation { get; set; }

        /// <summary>
        /// Updated profile data
        /// </summary>
        public UserProfileDto? Profile { get; set; }
    }
}