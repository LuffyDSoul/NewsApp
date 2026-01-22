using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace NewsApp.Domain.UserProfile
{
    /// <summary>
    /// Repository interface for EmailVerificationToken entity
    /// </summary>
    public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken, Guid>
    {
        /// <summary>
        /// Find a valid (not used and not expired) token by user ID and new email
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newEmail">The new email being verified</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Valid token or null if not found</returns>
        Task<EmailVerificationToken?> FindValidTokenAsync(
            Guid userId,
            string newEmail,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Find a token by token value
        /// </summary>
        /// <param name="token">Token value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Token or null if not found</returns>
        Task<EmailVerificationToken?> FindByTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete all expired tokens for cleanup
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of deleted tokens</returns>
        Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete all unused tokens for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of deleted tokens</returns>
        Task<int> DeleteUnusedTokensForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
