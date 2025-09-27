using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using System.ComponentModel.DataAnnotations;

namespace NewsApp.Controllers
{
    /// <summary>
    /// Controlador personalizado para gestión de perfil de usuario con idioma preferido
    /// </summary>
    [ApiController]
    [Route("api/user-profile")] // ? CAMBIO: Nueva ruta para evitar conflicto
    [Authorize]
    public class ProfileController : AbpControllerBase
    {
        private readonly IdentityUserManager _userManager;
        private readonly ICurrentUser _currentUser;

        public ProfileController(
            IdentityUserManager userManager,
            ICurrentUser currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Obtiene el perfil del usuario actual incluyendo idioma preferido
        /// </summary>
        /// <returns>Perfil del usuario actual</returns>
        [HttpGet("my-profile")]
        public async Task<UserProfileDto> GetMyProfileAsync()
        {
            var currentUserId = _currentUser.GetId();
            var user = await _userManager.GetByIdAsync(currentUserId);

            // Obtener idioma preferido usando ExtraProperties
            var preferredLanguage = "en";
            if (user.ExtraProperties.ContainsKey("PreferredLanguage"))
            {
                preferredLanguage = user.ExtraProperties["PreferredLanguage"]?.ToString() ?? "en";
            }

            return new UserProfileDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Surname = user.Surname,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PreferredLanguage = preferredLanguage
            };
        }

        /// <summary>
        /// Actualiza el perfil del usuario actual incluyendo idioma preferido
        /// </summary>
        /// <param name="input">Datos de actualización del perfil</param>
        /// <returns>Resultado de la actualización</returns>
        [HttpPut("my-profile")]
        public async Task<IActionResult> UpdateMyProfileAsync([FromBody] UpdateUserProfileDto input)
        {
            var currentUserId = _currentUser.GetId();
            var user = await _userManager.GetByIdAsync(currentUserId);

            // Actualizar propiedades básicas usando ABP Identity Manager
            if (user.UserName != input.UserName)
            {
                await _userManager.SetUserNameAsync(user, input.UserName);
            }
            
            if (user.Email != input.Email)
            {
                await _userManager.SetEmailAsync(user, input.Email);
            }
            
            user.Name = input.Name;
            user.Surname = input.Surname;
            
            if (user.PhoneNumber != input.PhoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber ?? string.Empty);
            }

            // Actualizar idioma preferido usando ExtraProperties
            if (!string.IsNullOrEmpty(input.PreferredLanguage))
            {
                user.ExtraProperties["PreferredLanguage"] = input.PreferredLanguage;
            }

            // Guardar cambios
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Profile updated successfully" });
            }

            return BadRequest(new { message = "Failed to update profile", errors = result.Errors });
        }

        /// <summary>
        /// Cambia la contraseña del usuario current
        /// </summary>
        /// <param name="input">Datos del cambio de contraseña</param>
        /// <returns>Resultado del cambio</returns>
        [HttpPost("my-profile/change-password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto input)
        {
            var currentUserId = _currentUser.GetId();
            var user = await _userManager.GetByIdAsync(currentUserId);

            var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Password changed successfully" });
            }

            return BadRequest(new { message = "Failed to change password", errors = result.Errors });
        }
    }

    /// <summary>
    /// DTO para el perfil del usuario
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string PreferredLanguage { get; set; } = "en";
    }

    /// <summary>
    /// DTO para actualizar el perfil del usuario
    /// </summary>
    public class UpdateUserProfileDto
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(64)]
        public string? Name { get; set; }

        [StringLength(64)]
        public string? Surname { get; set; }

        [Phone]
        [StringLength(16)]
        public string? PhoneNumber { get; set; }

        [StringLength(10)]
        public string? PreferredLanguage { get; set; }
    }

    /// <summary>
    /// DTO para cambio de contraseña
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(128)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}