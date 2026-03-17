using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ReplicaSync.Core.Enums;
using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Services;

/// <summary>
/// Orchestrates data synchronization between Pleasanter instances.
/// </summary>
public class SyncEngine : ISyncEngine
{
    private readonly IPleasanterDbAccess _dbAccess;
    private readonly ISyncConfigRepository _configRepository;
    private readonly VersionHistoryService _versionHistoryService;
    private readonly ILogger<SyncEngine> _logger;
    private readonly ConcurrentDictionary<string, DateTime> _lastSyncTimes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncEngine"/> class.
    /// </summary>
    public SyncEngine(
        IPleasanterDbAccess dbAccess,
        ISyncConfigRepository configRepository,
        VersionHistoryService versionHistoryService,
        ILogger<SyncEngine> logger)
    {
        ArgumentNullException.ThrowIfNull(dbAccess);
        ArgumentNullException.ThrowIfNull(configRepository);
        ArgumentNullException.ThrowIfNull(versionHistoryService);
        ArgumentNullException.ThrowIfNull(logger);

        _dbAccess = dbAccess;
        _configRepository = configRepository;
        _versionHistoryService = versionHistoryService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SyncLogEntry> ExecuteSyncAsync(SyncDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var logEntry = new SyncLogEntry
        {
            SyncId = definition.SyncId,
            SourceInstanceId = definition.SourceInstance?.InstanceId ?? string.Empty,
            StartedAt = DateTime.UtcNow,
            Status = SyncStatus.Success
        };

        try
        {
            var source = definition.SourceInstance;
            if (source is null)
            {
                throw new InvalidOperationException($"Source instance not loaded for sync definition '{definition.SyncId}'.");
            }

            var referenceType = await _dbAccess.GetReferenceTypeAsync(
                source.ConnectionString, source.DbmsType, definition.SourceSiteId, cancellationToken).ConfigureAwait(false);

            var lastSyncKey = definition.SyncId;
            var lastSyncTime = _lastSyncTimes.GetOrAdd(
                lastSyncKey,
                _ => DateTime.UtcNow.AddSeconds(-definition.PollingIntervalSeconds));

            // Get all columns we need (union of all target effective columns + sync keys)
            var allNeededColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var syncKey in definition.GetSyncKeyColumnList())
            {
                allNeededColumns.Add(syncKey);
            }

            var targetColumnsMap = new Dictionary<int, IReadOnlyList<string>>();
            foreach (var targetMapping in definition.TargetMappings)
            {
                var effectiveColumns = SyncRuleEngine.GetEffectiveColumns(definition, targetMapping, referenceType);
                targetColumnsMap[targetMapping.Id] = effectiveColumns;
                foreach (var col in effectiveColumns)
                {
                    allNeededColumns.Add(col);
                }
            }

            // Source to Target sync
            var changedRecords = await _dbAccess.GetChangedRecordsAsync(
                source.ConnectionString, source.DbmsType,
                definition.SourceSiteId, referenceType,
                lastSyncTime, definition.SyncUserId,
                allNeededColumns.ToList(),
                cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Sync '{SyncId}': Detected {Count} changed records from source.",
                definition.SyncId, changedRecords.Count);

            foreach (var targetMapping in definition.TargetMappings)
            {
                if (!targetMapping.SourceToTargetEnabled)
                {
                    continue;
                }

                var target = targetMapping.TargetInstance;
                if (target is null)
                {
                    _logger.LogWarning("Target instance not loaded for mapping {MappingId}.", targetMapping.Id);
                    continue;
                }

                var effectiveColumns = targetColumnsMap[targetMapping.Id];

                foreach (var record in changedRecords)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var filteredRecord = SyncRuleEngine.FilterRecordColumns(record, effectiveColumns);

                        var syncKeyValues = definition.GetSyncKeyColumnList()
                            .Select(k => record.GetColumnValue(k))
                            .ToList();

                        var existingTarget = await _dbAccess.FindRecordBySyncKeyAsync(
                            target.ConnectionString, target.DbmsType,
                            targetMapping.TargetSiteId, referenceType,
                            definition.GetSyncKeyColumnList(), syncKeyValues,
                            cancellationToken).ConfigureAwait(false);

                        if (existingTarget is not null && ShouldSkipDueToConflict(definition, record, existingTarget, lastSyncTime))
                        {
                            logEntry.ConflictsDetected++;
                            if (definition.ConflictResolution == ConflictResolutionStrategy.ManualResolution)
                            {
                                _logger.LogWarning(
                                    "Conflict detected for sync '{SyncId}', record key values: {Keys}. Skipping for manual resolution.",
                                    definition.SyncId, string.Join(", ", syncKeyValues));
                                continue;
                            }
                        }

                        // Capture version history snapshot before overwriting
                        if (existingTarget is not null)
                        {
                            await _versionHistoryService.CaptureSnapshotAsync(
                                definition, target.InstanceId,
                                targetMapping.TargetSiteId, existingTarget,
                                cancellationToken).ConfigureAwait(false);
                        }

                        await _dbAccess.UpsertRecordAsync(
                            target.ConnectionString, target.DbmsType,
                            targetMapping.TargetSiteId, referenceType,
                            filteredRecord, existingTarget,
                            effectiveColumns, definition.SyncUserId,
                            cancellationToken).ConfigureAwait(false);

                        if (existingTarget is null)
                        {
                            logEntry.RecordsInserted++;
                        }
                        else
                        {
                            logEntry.RecordsUpdated++;

                            // Apply per-record version count retention
                            await _versionHistoryService.ApplyRetentionForRecordAsync(
                                definition, target.InstanceId,
                                targetMapping.TargetSiteId, existingTarget.RecordId,
                                cancellationToken).ConfigureAwait(false);
                        }

                        logEntry.RecordsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing record {RecordId} to target {TargetId}.",
                            record.RecordId, target.InstanceId);
                        logEntry.Status = SyncStatus.Failed;
                        logEntry.ErrorMessage = ex.Message;
                    }
                }

                // Sync deletions
                var deletedRecords = await _dbAccess.GetDeletedRecordsAsync(
                    source.ConnectionString, source.DbmsType,
                    definition.SourceSiteId, referenceType,
                    lastSyncTime, definition.GetSyncKeyColumnList(),
                    cancellationToken).ConfigureAwait(false);

                foreach (var deletedRecord in deletedRecords)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var syncKeyValues = definition.GetSyncKeyColumnList()
                            .Select(k => deletedRecord.GetColumnValue(k))
                            .ToList();

                        var targetRecord = await _dbAccess.FindRecordBySyncKeyAsync(
                            target.ConnectionString, target.DbmsType,
                            targetMapping.TargetSiteId, referenceType,
                            definition.GetSyncKeyColumnList(), syncKeyValues,
                            cancellationToken).ConfigureAwait(false);

                        if (targetRecord is not null)
                        {
                            await _dbAccess.DeleteRecordAsync(
                                target.ConnectionString, target.DbmsType,
                                targetMapping.TargetSiteId, referenceType,
                                targetRecord, definition.SyncUserId,
                                cancellationToken).ConfigureAwait(false);

                            logEntry.RecordsDeleted++;
                            logEntry.RecordsProcessed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing deletion for record key in target {TargetId}.", target.InstanceId);
                        logEntry.Status = SyncStatus.Failed;
                        logEntry.ErrorMessage = ex.Message;
                    }
                }
            }

            _lastSyncTimes.AddOrUpdate(lastSyncKey, DateTime.UtcNow, (_, _) => DateTime.UtcNow);

            // Apply time-based retention cleanup
            await _versionHistoryService.ApplyTimeBasedRetentionAsync(definition, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync '{SyncId}' failed.", definition.SyncId);
            logEntry.Status = SyncStatus.Failed;
            logEntry.ErrorMessage = ex.Message;
        }
        finally
        {
            logEntry.CompletedAt = DateTime.UtcNow;
        }

        await _configRepository.CreateLogEntryAsync(logEntry, cancellationToken).ConfigureAwait(false);

        return logEntry;
    }

    private static bool ShouldSkipDueToConflict(
        SyncDefinition definition,
        PleasanterRecord sourceRecord,
        PleasanterRecord targetRecord,
        DateTime lastSyncTime)
    {
        // If target was updated after last sync by someone other than sync user,
        // it means both source and target were edited = conflict
        if (targetRecord.UpdatedTime > lastSyncTime && targetRecord.Updator != definition.SyncUserId)
        {
            // SourceWins or LastWriteWins resolved differently
            if (definition.ConflictResolution == ConflictResolutionStrategy.LastWriteWins)
            {
                return targetRecord.UpdatedTime > sourceRecord.UpdatedTime;
            }

            // SourceWins - don't skip, overwrite
            // ManualResolution - skip (handled by caller)
            return definition.ConflictResolution == ConflictResolutionStrategy.ManualResolution;
        }

        return false;
    }
}
