using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.Domain.UserProfile
{
    /// <summary>
    /// Repository interface for UserPreferences entity
    /// </summary>
    public interface IUserPreferencesRepository : IRepository<UserPreferences, Guid>
    {
        /// <summary>
        /// Find user preferences by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeDetails">Whether to include related data</param>
        /// <returns>User preferences or null if not found</returns>
        Task<UserPreferences?> FindByUserIdAsync(Guid userId, bool includeDetails = false);

        /// <summary>
        /// Get user preferences by user ID, creating default if not exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includeDetails">Whether to include related data</param>
        /// <returns>User preferences</returns>
        Task<UserPreferences> GetOrCreateByUserIdAsync(Guid userId, bool includeDetails = false);
    }
}