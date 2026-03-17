using Microsoft.EntityFrameworkCore;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Data;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Repositories;

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
    public async Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppUsers
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AppUsers
            .FindAsync([id], cancellationToken)
            .ConfigureAwait(false);
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
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.AppUsers
            .FindAsync([id], cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<bool> AnyUsersExistAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppUsers
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RecordFailedLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.AppUsers
            .FindAsync([userId], cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            user.FailedLoginCount++;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task ResetFailedLoginCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.AppUsers
            .FindAsync([userId], cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            user.FailedLoginCount = 0;
            user.LockoutEndUtc = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task LockoutUserAsync(int userId, DateTime lockoutEndUtc, CancellationToken cancellationToken = default)
    {
        var user = await _context.AppUsers
            .FindAsync([userId], cancellationToken)
            .ConfigureAwait(false);

        if (user is not null)
        {
            user.LockoutEndUtc = lockoutEndUtc;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
