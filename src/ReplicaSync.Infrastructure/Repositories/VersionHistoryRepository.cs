using Microsoft.EntityFrameworkCore;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;
using ReplicaSync.Infrastructure.Data;

namespace ReplicaSync.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of version history repository.
/// </summary>
public class VersionHistoryRepository : IVersionHistoryRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionHistoryRepository"/> class.
    /// </summary>
    public VersionHistoryRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<RecordVersionHistory> CreateAsync(RecordVersionHistory entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        entry.CreatedAt = DateTime.UtcNow;
        _context.RecordVersionHistories.Add(entry);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entry;
    }

    /// <inheritdoc />
    public async Task<int> GetNextVersionNumberAsync(string syncId, string instanceId, long siteId, long recordId, CancellationToken cancellationToken = default)
    {
        var maxVersion = await _context.RecordVersionHistories
            .Where(v => v.SyncId == syncId && v.InstanceId == instanceId && v.SiteId == siteId && v.RecordId == recordId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken)
            .ConfigureAwait(false);

        return (maxVersion ?? 0) + 1;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecordVersionHistory>> GetByRecordAsync(string syncId, string instanceId, long siteId, long recordId, CancellationToken cancellationToken = default)
    {
        return await _context.RecordVersionHistories
            .AsNoTracking()
            .Where(v => v.SyncId == syncId && v.InstanceId == instanceId && v.SiteId == siteId && v.RecordId == recordId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> DeleteExcessVersionsAsync(string syncId, string instanceId, long siteId, long recordId, int maxVersions, CancellationToken cancellationToken = default)
    {
        var excessEntries = await _context.RecordVersionHistories
            .Where(v => v.SyncId == syncId && v.InstanceId == instanceId && v.SiteId == siteId && v.RecordId == recordId)
            .OrderByDescending(v => v.VersionNumber)
            .Skip(maxVersions)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (excessEntries.Count == 0)
        {
            return 0;
        }

        _context.RecordVersionHistories.RemoveRange(excessEntries);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return excessEntries.Count;
    }

    /// <inheritdoc />
    public async Task<int> DeleteOlderThanAsync(string syncId, DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldEntries = await _context.RecordVersionHistories
            .Where(v => v.SyncId == syncId && v.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (oldEntries.Count == 0)
        {
            return 0;
        }

        _context.RecordVersionHistories.RemoveRange(oldEntries);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return oldEntries.Count;
    }
}
