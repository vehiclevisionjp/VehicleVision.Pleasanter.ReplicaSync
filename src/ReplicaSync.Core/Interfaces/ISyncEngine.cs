using ReplicaSync.Core.Models;

namespace ReplicaSync.Core.Interfaces;

/// <summary>
/// Interface for the sync engine that orchestrates data synchronization.
/// </summary>
public interface ISyncEngine
{
    /// <summary>
    /// Executes a single sync cycle for a given definition.
    /// </summary>
    Task<SyncLogEntry> ExecuteSyncAsync(SyncDefinition definition, CancellationToken cancellationToken = default);
}
