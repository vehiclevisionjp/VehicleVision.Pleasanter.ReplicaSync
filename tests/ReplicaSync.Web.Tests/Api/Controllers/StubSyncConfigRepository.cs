using ReplicaSync.Core.Interfaces;
using ReplicaSync.Core.Models;

namespace ReplicaSync.Web.Tests.Api.Controllers;

/// <summary>
/// In-memory stub for ISyncConfigRepository used by API controller tests.
/// </summary>
internal sealed class StubSyncConfigRepository : ISyncConfigRepository
{
    private readonly List<SyncInstance> _instances = [];
    private readonly List<SyncDefinition> _definitions = [];
    private readonly List<SyncLogEntry> _logs = [];
    private int _nextInstanceId = 1;
    private int _nextDefinitionId = 1;
    private long _nextLogId = 1;

    public void SeedInstance(SyncInstance instance)
    {
        if (instance.Id == 0) instance.Id = _nextInstanceId++;
        else if (instance.Id >= _nextInstanceId) _nextInstanceId = instance.Id + 1;
        _instances.Add(instance);
    }

    public void SeedDefinition(SyncDefinition definition)
    {
        if (definition.Id == 0) definition.Id = _nextDefinitionId++;
        else if (definition.Id >= _nextDefinitionId) _nextDefinitionId = definition.Id + 1;
        _definitions.Add(definition);
    }

    public void SeedLog(SyncLogEntry log)
    {
        if (log.Id == 0) log.Id = _nextLogId++;
        else if (log.Id >= _nextLogId) _nextLogId = log.Id + 1;
        _logs.Add(log);
    }

    // SyncInstance operations

    public Task<IReadOnlyList<SyncInstance>> GetAllInstancesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SyncInstance>>(_instances.ToList());

    public Task<SyncInstance?> GetInstanceByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_instances.Find(i => i.Id == id));

    public Task<SyncInstance> CreateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default)
    {
        instance.Id = _nextInstanceId++;
        instance.CreatedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;
        _instances.Add(instance);
        return Task.FromResult(instance);
    }

    public Task<SyncInstance> UpdateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default)
    {
        instance.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(instance);
    }

    public Task DeleteInstanceAsync(int id, CancellationToken cancellationToken = default)
    {
        _instances.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    // SyncDefinition operations

    public Task<IReadOnlyList<SyncDefinition>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SyncDefinition>>(_definitions.ToList());

    public Task<IReadOnlyList<SyncDefinition>> GetEnabledDefinitionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SyncDefinition>>(_definitions.Where(d => d.IsEnabled).ToList());

    public Task<SyncDefinition?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_definitions.Find(d => d.Id == id));

    public Task<SyncDefinition> CreateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default)
    {
        definition.Id = _nextDefinitionId++;
        definition.CreatedAt = DateTime.UtcNow;
        definition.UpdatedAt = DateTime.UtcNow;
        _definitions.Add(definition);
        return Task.FromResult(definition);
    }

    public Task<SyncDefinition> UpdateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default)
    {
        definition.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(definition);
    }

    public Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default)
    {
        _definitions.RemoveAll(d => d.Id == id);
        return Task.CompletedTask;
    }

    // SyncLogEntry operations

    public Task<IReadOnlyList<SyncLogEntry>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SyncLogEntry>>(_logs.OrderByDescending(l => l.StartedAt).Take(count).ToList());

    public Task<SyncLogEntry> CreateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        logEntry.Id = _nextLogId++;
        _logs.Add(logEntry);
        return Task.FromResult(logEntry);
    }

    public Task<SyncLogEntry> UpdateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default)
        => Task.FromResult(logEntry);
}
