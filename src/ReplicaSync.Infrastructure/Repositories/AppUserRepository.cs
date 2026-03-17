using Microsoft.EntityFrameworkCore;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Infrastructure.Data;

namespace ReplicaSync.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of application user repository.
/// </summary>
public class AppUserRepository : IAppUserRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppUserRepository"/> class.
    /// </summary>
    public AppUserRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AppUser> CreateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user;
    }

    /// <inheritdoc />
    public async Task<AppUser> UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.UpdatedAt = DateTime.UtcNow;
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user;
    }

    /// <inheritdoc />
    public async Task<bool> AnyUsersExistAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppUsers
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
