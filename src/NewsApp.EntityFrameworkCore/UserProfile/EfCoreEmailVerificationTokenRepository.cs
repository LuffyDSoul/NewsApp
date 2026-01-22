using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewsApp.Domain.UserProfile;
using NewsApp.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace NewsApp.UserProfile
{
    /// <summary>
    /// Entity Framework implementation of IEmailVerificationTokenRepository
    /// </summary>
    public class EfCoreEmailVerificationTokenRepository 
        : EfCoreRepository<NewsAppDbContext, EmailVerificationToken, Guid>, 
          IEmailVerificationTokenRepository
    {
        public EfCoreEmailVerificationTokenRepository(IDbContextProvider<NewsAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<EmailVerificationToken?> FindValidTokenAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default)
        {
            var queryable = await GetQueryableAsync();
            var now = DateTime.UtcNow;
            
            return await queryable
                .Where(x => x.UserId == userId 
                    && x.NewEmail == newEmail 
                    && !x.IsUsed 
                    && x.ExpirationDate > now)
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<EmailVerificationToken?> FindByTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var queryable = await GetQueryableAsync();
            
            return await queryable
                .Where(x => x.Token == token)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var now = DateTime.UtcNow;
            
            return await dbContext.Set<EmailVerificationToken>()
                .Where(x => x.ExpirationDate <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<int> DeleteUnusedTokensForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            
            return await dbContext.Set<EmailVerificationToken>()
                .Where(x => x.UserId == userId && !x.IsUsed)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public override async Task<IQueryable<EmailVerificationToken>> WithDetailsAsync()
        {
            return await GetQueryableAsync();
        }
    }
}
