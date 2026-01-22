using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsApp.Domain.UserProfile;
using NewsApp.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace NewsApp.UserProfile
{
    /// <summary>
    /// Entity Framework implementation of IUserPreferencesRepository
    /// </summary>
    public class EfCoreUserPreferencesRepository : EfCoreRepository<NewsAppDbContext, UserPreferences, Guid>, IUserPreferencesRepository
    {
        public EfCoreUserPreferencesRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<UserPreferences?> FindByUserIdAsync(Guid userId, bool includeDetails = false)
        {
            var queryable = await GetQueryableAsync();
            
            return await queryable
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<UserPreferences> GetOrCreateByUserIdAsync(Guid userId, bool includeDetails = false)
        {
            var existing = await FindByUserIdAsync(userId, includeDetails);
            if (existing != null)
            {
                return existing;
            }

            // Create default preferences for the user
            var newPreferences = new UserPreferences(
                GuidGenerator.Create(),
                userId,
                "en",
                "English"
            );

            return await InsertAsync(newPreferences, autoSave: true);
        }

        public override async Task<IQueryable<UserPreferences>> WithDetailsAsync()
        {
            return (await GetQueryableAsync());
        }
    }
}