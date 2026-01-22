using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsApp.Domain.UserProfile;
using NewsApp.Email;
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
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserProfileAppService(
            IIdentityUserRepository userRepository,
            IdentityUserManager userManager,
            IUserPreferencesRepository userPreferencesRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IPasswordHasher<IdentityUser> passwordHasher,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _userPreferencesRepository = userPreferencesRepository;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _configuration = configuration;
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

                Logger.LogInformation("UpdateProfile called. Current email in DB: {OldEmail}, New email from input: {NewEmail}", 
                    user.Email, input.Email);

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
                Logger.LogInformation("Email changed: {EmailChanged}. Old: {OldEmail}, New: {NewEmail}", 
                    emailChanged, user.Email, input.Email);

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

                    // Instead of updating email directly, send verification email
                    // Delete any existing unused tokens for this user
                    await _emailVerificationTokenRepository.DeleteUnusedTokensForUserAsync(currentUserId);

                    // Generate token
                    var token = Guid.NewGuid().ToString("N");
                    var expirationDate = DateTime.UtcNow.AddHours(24);

                    // Create verification token
                    var verificationToken = new Domain.UserProfile.EmailVerificationToken(
                        GuidGenerator.Create(),
                        currentUserId,
                        input.Email,
                        token,
                        expirationDate
                    );

                    await _emailVerificationTokenRepository.InsertAsync(verificationToken, autoSave: true);

                    // Build confirmation URL
                    var clientUrl = _configuration["App:ClientUrl"] ?? "http://localhost:4200";
                    var confirmationUrl = $"{clientUrl}/confirm-email?token={Uri.EscapeDataString(token)}";

                    // Build email HTML
                    var emailBody = BuildEmailChangeConfirmationHtml(user.UserName ?? input.Email, input.Email, confirmationUrl);

                    Logger.LogInformation("Sending email verification to new address: {Email}", input.Email);

                    // Send email to the NEW email address
                    await _emailService.SendEmailAsync(
                        to: input.Email,
                        subject: "Confirma tu nueva dirección de email - NewsApp",
                        body: emailBody,
                        isHtml: true
                    );

                    Logger.LogInformation("Email verification sent successfully to {Email}", input.Email);
                }

                // Update other user properties (but NOT email yet)
                await _userManager.SetUserNameAsync(user, input.UserName);
                // Do NOT update email here - it will be updated after verification
                
                Logger.LogInformation("After SetUserNameAsync, user.UserName is now: {UserName}", user.UserName);
                
                user.Name = input.Name;
                user.Surname = input.Surname;
                await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);

                // Use UserManager to update
                await _userManager.UpdateAsync(user);
                Logger.LogInformation("User updated via UserManager (email NOT changed yet)");
                

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
                    Message = emailChanged 
                        ? "Perfil actualizado. Se ha enviado un correo de verificación a tu nueva dirección de email. Por favor, confírmalo para completar el cambio."
                        : L["ProfileUpdatedSuccessfully"],
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
                Logger.LogInformation("=== SendEmailConfirmation START === UserId: {UserId}", currentUserId);
                
                // Use GetAsync to ensure we're getting the latest data from database
                var user = await _userRepository.GetAsync(currentUserId);
                
                Logger.LogInformation("User loaded from database - UserId: {UserId}, Email: {Email}, EmailConfirmed: {EmailConfirmed}", 
                    user.Id, user.Email, user.EmailConfirmed);

                Logger.LogInformation("SendEmailConfirmation called for user {UserId}. Current email in DB: {Email}, EmailConfirmed: {EmailConfirmed}", 
                    user.Id, user.Email, user.EmailConfirmed);

                if (user.EmailConfirmed)
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = L["EmailAlreadyConfirmed"]
                    };
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    Logger.LogWarning("Cannot send email confirmation: email address is empty for user {UserId}", user.Id);
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = "No se encontró una dirección de email. Por favor, actualiza tu perfil primero."
                    };
                }

                // Validate email format
                if (!user.Email.Contains("@") || !user.Email.Contains("."))
                {
                    Logger.LogWarning("Invalid email format for user {UserId}: {Email}", user.Id, user.Email);
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = "El formato del email no es válido"
                    };
                }

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // Build confirmation URL
                var clientUrl = _configuration["App:ClientUrl"] ?? "http://localhost:4200";
                var confirmationUrl = $"{clientUrl}/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                // Build email HTML
                var emailBody = BuildEmailConfirmationHtml(user.UserName ?? user.Email, confirmationUrl);

                Logger.LogInformation("Preparing to send confirmation email - UserId: {UserId}, TO: {EmailTo}, FROM: {EmailFrom}", 
                    user.Id, user.Email, _configuration["Email:DefaultFromAddress"]);

                // Send email - explicitly passing the user's email as recipient
                var recipientEmail = user.Email;
                await _emailService.SendEmailAsync(
                    to: recipientEmail,
                    subject: "Confirma tu dirección de email - NewsApp",
                    body: emailBody,
                    isHtml: true
                );

                Logger.LogInformation("Email confirmation successfully sent to user {UserId} at email address: {Email}", user.Id, recipientEmail);

                return new ProfileUpdateResultDto
                {
                    Success = true,
                    Message = "Correo de confirmación enviado exitosamente. Por favor, revisa tu bandeja de entrada."
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to send email confirmation for user {UserId}", CurrentUser.GetId());
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = "Error al enviar el correo de confirmación. Por favor, intenta nuevamente."
                };
            }
        }

        private string BuildEmailConfirmationHtml(string userName, string confirmationUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #667eea; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }");
            sb.AppendLine(".content { background: white; padding: 30px; border: 1px solid #ddd; border-top: none; }");
            sb.AppendLine(".button { display: inline-block; padding: 12px 30px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; color: #666; font-size: 0.9em; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>📧 Confirma tu Email</h1>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<p>Hola {userName},</p>");
            sb.AppendLine("<p>Gracias por registrarte en <strong>NewsApp</strong>.</p>");
            sb.AppendLine("<p>Para completar tu registro y empezar a recibir notificaciones de noticias, por favor confirma tu dirección de email haciendo clic en el siguiente botón:</p>");
            sb.AppendLine($"<center><a href='{confirmationUrl}' class='button'>Confirmar Email</a></center>");
            sb.AppendLine("<p>O copia y pega el siguiente enlace en tu navegador:</p>");
            sb.AppendLine($"<p style='word-break: break-all; color: #667eea;'>{confirmationUrl}</p>");
            sb.AppendLine("<p><strong>Nota:</strong> Este enlace es válido por 24 horas.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Si no solicitaste este correo, puedes ignorarlo.</p>");
            sb.AppendLine("<p>© 2024 NewsApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private string BuildEmailChangeConfirmationHtml(string userName, string newEmail, string confirmationUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            sb.AppendLine(".container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { background-color: #667eea; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }");
            sb.AppendLine(".content { background: white; padding: 30px; border: 1px solid #ddd; border-top: none; }");
            sb.AppendLine(".button { display: inline-block; padding: 12px 30px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }");
            sb.AppendLine(".footer { text-align: center; margin-top: 20px; color: #666; font-size: 0.9em; }");
            sb.AppendLine(".new-email { background-color: #f0f0f0; padding: 10px; border-radius: 5px; font-weight: bold; margin: 10px 0; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>🔄 Confirma tu Cambio de Email</h1>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<div class='content'>");
            sb.AppendLine($"<p>Hola {userName},</p>");
            sb.AppendLine("<p>Has solicitado cambiar tu dirección de email en <strong>NewsApp</strong> a:</p>");
            sb.AppendLine($"<div class='new-email'>{newEmail}</div>");
            sb.AppendLine("<p>Para completar este cambio, por favor confirma tu nueva dirección de email haciendo clic en el siguiente botón:</p>");
            sb.AppendLine($"<center><a href='{confirmationUrl}' class='button'>Confirmar Nuevo Email</a></center>");
            sb.AppendLine("<p>O copia y pega el siguiente enlace en tu navegador:</p>");
            sb.AppendLine($"<p style='word-break: break-all; color: #667eea;'>{confirmationUrl}</p>");
            sb.AppendLine("<p><strong>Nota:</strong> Este enlace es válido por 24 horas.</p>");
            sb.AppendLine("<p><strong>Importante:</strong> Tu email actual NO se cambiará hasta que confirmes el nuevo.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<p>Si no solicitaste este cambio, puedes ignorar este correo y tu email actual permanecerá sin cambios.</p>");
            sb.AppendLine("<p>© 2024 NewsApp. Todos los derechos reservados.</p>");
            sb.AppendLine("</div>");
            
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        [AllowAnonymous]
        public async Task<ProfileUpdateResultDto> ConfirmEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = "Token inválido"
                    };
                }

                // Find the token
                var verificationToken = await _emailVerificationTokenRepository.FindByTokenAsync(token);
                
                if (verificationToken == null)
                {
                    Logger.LogWarning("Email verification token not found: {Token}", token);
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = "Token de verificación no encontrado o inválido"
                    };
                }

                // Check if token is valid
                if (!verificationToken.IsValid())
                {
                    Logger.LogWarning("Email verification token is invalid or expired for user {UserId}", verificationToken.UserId);
                    
                    if (verificationToken.IsUsed)
                    {
                        return new ProfileUpdateResultDto
                        {
                            Success = false,
                            Message = "Este token ya ha sido utilizado"
                        };
                    }
                    
                    if (verificationToken.IsExpired())
                    {
                        return new ProfileUpdateResultDto
                        {
                            Success = false,
                            Message = "El token ha expirado. Por favor, solicita un nuevo cambio de email"
                        };
                    }
                }

                // Get the user
                var user = await _userRepository.GetAsync(verificationToken.UserId);

                // Check if the new email is still available
                var existingUserByEmail = await _userRepository.FindByNormalizedEmailAsync(
                    _userManager.NormalizeEmail(verificationToken.NewEmail));
                
                if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                {
                    Logger.LogWarning("Email {Email} is now taken by another user", verificationToken.NewEmail);
                    return new ProfileUpdateResultDto
                    {
                        Success = false,
                        Message = "Esta dirección de email ya está en uso por otro usuario"
                    };
                }

                // Update the user's email
                await _userManager.SetEmailAsync(user, verificationToken.NewEmail);
                user.SetEmailConfirmed(true);
                await _userManager.UpdateAsync(user);

                // Mark token as used
                verificationToken.MarkAsUsed();
                await _emailVerificationTokenRepository.UpdateAsync(verificationToken, autoSave: true);

                Logger.LogInformation("Email successfully changed and confirmed for user {UserId} to {Email}", 
                    user.Id, verificationToken.NewEmail);

                return new ProfileUpdateResultDto
                {
                    Success = true,
                    Message = "Email confirmado exitosamente. Tu dirección de email ha sido actualizada."
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to confirm email with token");
                return new ProfileUpdateResultDto
                {
                    Success = false,
                    Message = "Error al confirmar el email. Por favor, intenta nuevamente."
                };
            }
        }
    }
}