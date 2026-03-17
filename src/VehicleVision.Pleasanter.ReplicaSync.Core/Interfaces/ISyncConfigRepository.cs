using VehicleVision.Pleasanter.ReplicaSync.Core.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;

/// <summary>
/// Repository for managing sync configuration data.
/// </summary>
public interface ISyncConfigRepository
{
    // SyncInstance operations

    /// <summary>Gets all sync instances.</summary>
    Task<IReadOnlyList<SyncInstance>> GetAllInstancesAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a sync instance by its ID.</summary>
    Task<SyncInstance?> GetInstanceByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new sync instance.</summary>
    Task<SyncInstance> CreateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing sync instance.</summary>
    Task<SyncInstance> UpdateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sync instance by its ID.</summary>
    Task DeleteInstanceAsync(int id, CancellationToken cancellationToken = default);

    // SyncDefinition operations

    /// <summary>Gets all sync definitions.</summary>
    Task<IReadOnlyList<SyncDefinition>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets all enabled sync definitions.</summary>
    Task<IReadOnlyList<SyncDefinition>> GetEnabledDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a sync definition by its ID.</summary>
    Task<SyncDefinition?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a new sync definition.</summary>
    Task<SyncDefinition> CreateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing sync definition.</summary>
    Task<SyncDefinition> UpdateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>Deletes a sync definition by its ID.</summary>
    Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default);

    // SyncLogEntry operations

    /// <summary>Gets the most recent sync log entries.</summary>
    Task<IReadOnlyList<SyncLogEntry>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default);

    /// <summary>Creates a new sync log entry.</summary>
    Task<SyncLogEntry> CreateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing sync log entry.</summary>
    Task<SyncLogEntry> UpdateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default);
}
