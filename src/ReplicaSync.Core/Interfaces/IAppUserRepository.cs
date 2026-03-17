using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Interfaces;

/// <summary>
/// Repository for managing application users.
/// </summary>
public interface IAppUserRepository
{
    /// <summary>Gets all users.</summary>
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a user by ID.</summary>
    Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a user by username (case-insensitive).</summary>
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user.</summary>
    Task<AppUser> CreateAsync(AppUser user, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing user.</summary>
    Task<AppUser> UpdateAsync(AppUser user, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user by ID.</summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Checks whether any users exist in the database.</summary>
    Task<bool> AnyUsersExistAsync(CancellationToken cancellationToken = default);
}
