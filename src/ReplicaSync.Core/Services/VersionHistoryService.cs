using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Services;

/// <summary>
/// Manages record version history creation and retention cleanup.
/// </summary>
public class VersionHistoryService
{
    private readonly IVersionHistoryRepository _repository;
    private readonly ILogger<VersionHistoryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionHistoryService"/> class.
    /// </summary>
    public VersionHistoryService(
        IVersionHistoryRepository repository,
        ILogger<VersionHistoryService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);

        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Captures a snapshot of the existing target record before it is overwritten by a sync operation.
    /// </summary>
    /// <param name="definition">The sync definition with version history settings.</param>
    /// <param name="instanceId">The instance ID where the record resides.</param>
    /// <param name="siteId">The Pleasanter site ID.</param>
    /// <param name="existingRecord">The existing record to snapshot (before overwrite).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created version history entry, or null if version history is disabled.</returns>
    public Task<RecordVersionHistory?> CaptureSnapshotAsync(
        SyncDefinition definition,
        string instanceId,
        long siteId,
        PleasanterRecord existingRecord,
        CancellationToken cancellationToken = default)
    {
        return CaptureSnapshotCoreAsync(definition, instanceId, siteId, existingRecord, isDeleteSnapshot: false, cancellationToken);
    }

    /// <summary>
    /// Captures a snapshot of the existing target record before it is deleted by a sync operation.
    /// </summary>
    /// <param name="definition">The sync definition with version history settings.</param>
    /// <param name="instanceId">The instance ID where the record resides.</param>
    /// <param name="siteId">The Pleasanter site ID.</param>
    /// <param name="existingRecord">The existing record to snapshot (before deletion).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The created version history entry, or null if version history is disabled.</returns>
    public Task<RecordVersionHistory?> CaptureDeleteSnapshotAsync(
        SyncDefinition definition,
        string instanceId,
        long siteId,
        PleasanterRecord existingRecord,
        CancellationToken cancellationToken = default)
    {
        return CaptureSnapshotCoreAsync(definition, instanceId, siteId, existingRecord, isDeleteSnapshot: true, cancellationToken);
    }

    private async Task<RecordVersionHistory?> CaptureSnapshotCoreAsync(
        SyncDefinition definition,
        string instanceId,
        long siteId,
        PleasanterRecord existingRecord,
        bool isDeleteSnapshot,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(existingRecord);

        if (!definition.VersionHistoryEnabled)
        {
            return null;
        }

        var nextVersion = await _repository.GetNextVersionNumberAsync(
            definition.SyncId, instanceId, siteId, existingRecord.RecordId, cancellationToken).ConfigureAwait(false);

        var entry = new RecordVersionHistory
        {
            SyncId = definition.SyncId,
            InstanceId = instanceId,
            SiteId = siteId,
            RecordId = existingRecord.RecordId,
            VersionNumber = nextVersion,
            Title = existingRecord.Title,
            Body = existingRecord.Body,
            ColumnSnapshotJson = JsonSerializer.Serialize(existingRecord.ColumnValues),
            ChangedBy = existingRecord.Updator,
            ChangedAt = existingRecord.UpdatedTime,
            IsDeleteSnapshot = isDeleteSnapshot
        };

        var created = await _repository.CreateAsync(entry, cancellationToken).ConfigureAwait(false);

        var snapshotType = isDeleteSnapshot ? "delete" : "update";
        _logger.LogDebug(
            "Captured {SnapshotType} version {Version} for record {RecordId} on instance '{InstanceId}' site {SiteId}.",
            snapshotType, nextVersion, existingRecord.RecordId, instanceId, siteId);

        return created;
    }

    /// <summary>
    /// Applies retention policy for a specific record after a new version is captured.
    /// Enforces the "earlier of" logic: versions exceeding maxVersions OR older than maxDays are deleted.
    /// </summary>
    /// <param name="definition">The sync definition with retention settings.</param>
    /// <param name="instanceId">The instance ID.</param>
    /// <param name="siteId">The site ID.</param>
    /// <param name="recordId">The record ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task ApplyRetentionForRecordAsync(
        SyncDefinition definition,
        string instanceId,
        long siteId,
        long recordId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.VersionHistoryMaxVersions.HasValue)
        {
            var deletedByCount = await _repository.DeleteExcessVersionsAsync(
                definition.SyncId, instanceId, siteId, recordId,
                definition.VersionHistoryMaxVersions.Value, cancellationToken).ConfigureAwait(false);

            if (deletedByCount > 0)
            {
                _logger.LogDebug(
                    "Retention: Deleted {Count} excess versions for record {RecordId} (max: {Max}).",
                    deletedByCount, recordId, definition.VersionHistoryMaxVersions.Value);
            }
        }
    }

    /// <summary>
    /// Applies time-based retention cleanup for an entire sync definition.
    /// This should be called periodically (e.g., after each sync cycle).
    /// </summary>
    /// <param name="definition">The sync definition with retention settings.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task ApplyTimeBasedRetentionAsync(
        SyncDefinition definition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!definition.VersionHistoryMaxDays.HasValue)
        {
            return;
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-definition.VersionHistoryMaxDays.Value);
        var deletedByAge = await _repository.DeleteOlderThanAsync(
            definition.SyncId, cutoffDate, cancellationToken).ConfigureAwait(false);

        if (deletedByAge > 0)
        {
            _logger.LogInformation(
                "Retention: Deleted {Count} old version history entries for sync '{SyncId}' (older than {Days} days).",
                deletedByAge, definition.SyncId, definition.VersionHistoryMaxDays.Value);
        }
    }
}
