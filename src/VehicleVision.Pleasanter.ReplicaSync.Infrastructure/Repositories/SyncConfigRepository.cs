using Microsoft.EntityFrameworkCore;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Data;

namespace VehicleVision.Pleasanter.ReplicaSync.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of sync configuration repository.
/// </summary>
public class SyncConfigRepository : ISyncConfigRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncConfigRepository"/> class.
    /// </summary>
    public SyncConfigRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncInstance>> GetAllInstancesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncInstances
            .AsNoTracking()
            .OrderBy(i => i.DisplayName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SyncInstance?> GetInstanceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncInstances
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SyncInstance> CreateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        instance.CreatedAt = DateTime.UtcNow;
        instance.UpdatedAt = DateTime.UtcNow;
        _context.SyncInstances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return instance;
    }

    /// <inheritdoc />
    public async Task<SyncInstance> UpdateInstanceAsync(SyncInstance instance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        instance.UpdatedAt = DateTime.UtcNow;
        _context.SyncInstances.Update(instance);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return instance;
    }

    /// <inheritdoc />
    public async Task DeleteInstanceAsync(int id, CancellationToken cancellationToken = default)
    {
        var instance = await _context.SyncInstances
            .FindAsync([id], cancellationToken)
            .ConfigureAwait(false);
        if (instance is not null)
        {
            _context.SyncInstances.Remove(instance);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncDefinition>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncDefinitions
            .AsNoTracking()
            .Include(d => d.SourceInstance)
            .Include(d => d.TargetMappings)
                .ThenInclude(t => t.TargetInstance)
            .OrderBy(d => d.SyncId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncDefinition>> GetEnabledDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncDefinitions
            .AsNoTracking()
            .Include(d => d.SourceInstance)
            .Include(d => d.TargetMappings)
                .ThenInclude(t => t.TargetInstance)
            .Where(d => d.IsEnabled)
            .OrderBy(d => d.SyncId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SyncDefinition?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncDefinitions
            .Include(d => d.SourceInstance)
            .Include(d => d.TargetMappings)
                .ThenInclude(t => t.TargetInstance)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SyncDefinition> CreateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.CreatedAt = DateTime.UtcNow;
        definition.UpdatedAt = DateTime.UtcNow;
        _context.SyncDefinitions.Add(definition);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return definition;
    }

    /// <inheritdoc />
    public async Task<SyncDefinition> UpdateDefinitionAsync(SyncDefinition definition, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.UpdatedAt = DateTime.UtcNow;
        _context.SyncDefinitions.Update(definition);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return definition;
    }

    /// <inheritdoc />
    public async Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default)
    {
        var definition = await _context.SyncDefinitions
            .Include(d => d.TargetMappings)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (definition is not null)
        {
            _context.SyncDefinitions.Remove(definition);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SyncLogEntry>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SyncLogEntries
            .AsNoTracking()
            .OrderByDescending(l => l.StartedAt)
            .Take(count)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SyncLogEntry> CreateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        _context.SyncLogEntries.Add(logEntry);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return logEntry;
    }

    /// <inheritdoc />
    public async Task<SyncLogEntry> UpdateLogEntryAsync(SyncLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        _context.SyncLogEntries.Update(logEntry);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return logEntry;
    }
}
