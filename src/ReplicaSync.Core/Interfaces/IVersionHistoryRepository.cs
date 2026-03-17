using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Interfaces;

/// <summary>
/// Repository for managing record version history.
/// </summary>
public interface IVersionHistoryRepository
{
    /// <summary>
    /// Creates a new version history entry.
    /// </summary>
    Task<RecordVersionHistory> CreateAsync(RecordVersionHistory entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next version number for a specific record.
    /// </summary>
    Task<int> GetNextVersionNumberAsync(string syncId, string instanceId, long siteId, long recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets version history entries for a specific record, ordered by version number descending.
    /// </summary>
    Task<IReadOnlyList<RecordVersionHistory>> GetByRecordAsync(string syncId, string instanceId, long siteId, long recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes version history entries that exceed the maximum version count for a specific record.
    /// Returns the number of entries deleted.
    /// </summary>
    Task<int> DeleteExcessVersionsAsync(string syncId, string instanceId, long siteId, long recordId, int maxVersions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes version history entries older than the specified cutoff date for a given sync definition.
    /// Returns the number of entries deleted.
    /// </summary>
    Task<int> DeleteOlderThanAsync(string syncId, DateTime cutoffDate, CancellationToken cancellationToken = default);
}
