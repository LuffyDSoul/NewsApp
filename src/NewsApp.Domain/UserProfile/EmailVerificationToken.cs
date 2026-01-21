using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.UserProfile
{
    /// <summary>
    /// Entity for storing email verification tokens when users change their email
    /// </summary>
    public class EmailVerificationToken : CreationAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// ID of the user requesting email change
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The new email address to be verified
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string NewEmail { get; set; } = string.Empty;

        /// <summary>
        /// The verification token
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// When this token expires
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Whether this token has been used
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// When the token was used (if applicable)
        /// </summary>
        public DateTime? UsedDate { get; set; }

        protected EmailVerificationToken()
        {
            // For EF Core
        }

        public EmailVerificationToken(
            Guid id,
            Guid userId,
            string newEmail,
            string token,
            DateTime expirationDate) : base(id)
        {
            UserId = userId;
            SetNewEmail(newEmail);
            SetToken(token);
            ExpirationDate = expirationDate;
            IsUsed = false;
        }

        /// <summary>
        /// Set the new email address
        /// </summary>
        public void SetNewEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("Email cannot be empty", nameof(newEmail));

            if (!newEmail.Contains("@"))
                throw new ArgumentException("Invalid email format", nameof(newEmail));

            NewEmail = newEmail;
        }

        /// <summary>
        /// Set the verification token
        /// </summary>
        public void SetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", nameof(token));

            Token = token;
        }

        /// <summary>
        /// Check if the token is valid (not expired and not used)
        /// </summary>
        public bool IsValid()
        {
            return !IsUsed && DateTime.UtcNow < ExpirationDate;
        }

        /// <summary>
        /// Mark the token as used
        /// </summary>
        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if the token has expired
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpirationDate;
        }
    }
}
