using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Security;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;

/// <summary>
/// API controller for managing sync definitions.
/// </summary>
[ApiController]
[Route("api/sync-definitions")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public sealed class SyncDefinitionsController : ControllerBase
{
    private readonly ISyncConfigRepository _syncRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncDefinitionsController"/> class.
    /// </summary>
    public SyncDefinitionsController(ISyncConfigRepository syncRepo)
    {
        _syncRepo = syncRepo;
    }

    /// <summary>Gets all sync definitions.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SyncDefinitionResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var definitions = await _syncRepo.GetAllDefinitionsAsync(cancellationToken);
        return Ok(definitions.Select(ToResponse));
    }

    /// <summary>Gets a sync definition by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SyncDefinitionResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var definition = await _syncRepo.GetDefinitionByIdAsync(id, cancellationToken);
        if (definition is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(definition));
    }

    /// <summary>Creates a new sync definition.</summary>
    [HttpPost]
    public async Task<ActionResult<SyncDefinitionResponse>> CreateAsync(
        [FromBody] CreateSyncDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var definition = new SyncDefinition
        {
            SyncId = request.SyncId,
            Description = request.Description,
            Topology = request.Topology,
            ConflictResolution = request.ConflictResolution,
            ChangeDetectionMethod = request.ChangeDetectionMethod,
            PollingIntervalSeconds = request.PollingIntervalSeconds,
            SyncUserId = request.SyncUserId,
            SyncUserName = request.SyncUserName,
            SourceInstanceId = request.SourceInstanceId,
            SourceSiteId = request.SourceSiteId,
            IsEnabled = request.IsEnabled,
            SyncKeyColumns = request.SyncKeyColumns,
            IncludeColumns = request.IncludeColumns,
            ExcludeColumns = request.ExcludeColumns,
            RecordFilterInclude = request.RecordFilterInclude,
            RecordFilterExclude = request.RecordFilterExclude,
            AttachmentsEnabled = request.AttachmentsEnabled,
            AttachmentsStorageType = request.AttachmentsStorageType,
            VersionHistoryEnabled = request.VersionHistoryEnabled,
            VersionHistoryMaxVersions = request.VersionHistoryMaxVersions,
            VersionHistoryMaxDays = request.VersionHistoryMaxDays,
            TargetMappings = request.TargetMappings.Select(m => new SyncTargetMapping
            {
                TargetInstanceId = m.TargetInstanceId,
                TargetSiteId = m.TargetSiteId,
                SourceToTargetEnabled = m.SourceToTargetEnabled,
                TargetToSourceEnabled = m.TargetToSourceEnabled,
                TargetToSourceExcludeColumns = m.TargetToSourceExcludeColumns,
                TargetExcludeColumns = m.TargetExcludeColumns,
                RecordFilterIncludeOverride = m.RecordFilterIncludeOverride,
                RecordFilterExcludeOverride = m.RecordFilterExcludeOverride,
            }).ToList(),
        };

        var created = await _syncRepo.CreateDefinitionAsync(definition, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>Updates an existing sync definition.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SyncDefinitionResponse>> UpdateAsync(
        int id,
        [FromBody] UpdateSyncDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var definition = await _syncRepo.GetDefinitionByIdAsync(id, cancellationToken);
        if (definition is null)
        {
            return NotFound();
        }

        definition.SyncId = request.SyncId;
        definition.Description = request.Description;
        definition.Topology = request.Topology;
        definition.ConflictResolution = request.ConflictResolution;
        definition.ChangeDetectionMethod = request.ChangeDetectionMethod;
        definition.PollingIntervalSeconds = request.PollingIntervalSeconds;
        definition.SyncUserId = request.SyncUserId;
        definition.SyncUserName = request.SyncUserName;
        definition.SourceInstanceId = request.SourceInstanceId;
        definition.SourceSiteId = request.SourceSiteId;
        definition.IsEnabled = request.IsEnabled;
        definition.SyncKeyColumns = request.SyncKeyColumns;
        definition.IncludeColumns = request.IncludeColumns;
        definition.ExcludeColumns = request.ExcludeColumns;
        definition.RecordFilterInclude = request.RecordFilterInclude;
        definition.RecordFilterExclude = request.RecordFilterExclude;
        definition.AttachmentsEnabled = request.AttachmentsEnabled;
        definition.AttachmentsStorageType = request.AttachmentsStorageType;
        definition.VersionHistoryEnabled = request.VersionHistoryEnabled;
        definition.VersionHistoryMaxVersions = request.VersionHistoryMaxVersions;
        definition.VersionHistoryMaxDays = request.VersionHistoryMaxDays;

        // Replace target mappings
        definition.TargetMappings = request.TargetMappings.Select(m => new SyncTargetMapping
        {
            SyncDefinitionId = id,
            TargetInstanceId = m.TargetInstanceId,
            TargetSiteId = m.TargetSiteId,
            SourceToTargetEnabled = m.SourceToTargetEnabled,
            TargetToSourceEnabled = m.TargetToSourceEnabled,
            TargetToSourceExcludeColumns = m.TargetToSourceExcludeColumns,
            TargetExcludeColumns = m.TargetExcludeColumns,
            RecordFilterIncludeOverride = m.RecordFilterIncludeOverride,
            RecordFilterExcludeOverride = m.RecordFilterExcludeOverride,
        }).ToList();

        var updated = await _syncRepo.UpdateDefinitionAsync(definition, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Deletes a sync definition by ID.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var definition = await _syncRepo.GetDefinitionByIdAsync(id, cancellationToken);
        if (definition is null)
        {
            return NotFound();
        }

        await _syncRepo.DeleteDefinitionAsync(id, cancellationToken);
        return NoContent();
    }

    private static SyncDefinitionResponse ToResponse(SyncDefinition def) => new()
    {
        Id = def.Id,
        SyncId = def.SyncId,
        Description = def.Description,
        Topology = def.Topology,
        ConflictResolution = def.ConflictResolution,
        ChangeDetectionMethod = def.ChangeDetectionMethod,
        PollingIntervalSeconds = def.PollingIntervalSeconds,
        SyncUserId = def.SyncUserId,
        SyncUserName = def.SyncUserName,
        SourceInstanceId = def.SourceInstanceId,
        SourceSiteId = def.SourceSiteId,
        IsEnabled = def.IsEnabled,
        SyncKeyColumns = def.SyncKeyColumns,
        IncludeColumns = def.IncludeColumns,
        ExcludeColumns = def.ExcludeColumns,
        RecordFilterInclude = def.RecordFilterInclude,
        RecordFilterExclude = def.RecordFilterExclude,
        AttachmentsEnabled = def.AttachmentsEnabled,
        AttachmentsStorageType = def.AttachmentsStorageType,
        VersionHistoryEnabled = def.VersionHistoryEnabled,
        VersionHistoryMaxVersions = def.VersionHistoryMaxVersions,
        VersionHistoryMaxDays = def.VersionHistoryMaxDays,
        TargetMappings = def.TargetMappings.Select(m => new SyncTargetMappingResponse
        {
            Id = m.Id,
            TargetInstanceId = m.TargetInstanceId,
            TargetSiteId = m.TargetSiteId,
            SourceToTargetEnabled = m.SourceToTargetEnabled,
            TargetToSourceEnabled = m.TargetToSourceEnabled,
            TargetToSourceExcludeColumns = m.TargetToSourceExcludeColumns,
            TargetExcludeColumns = m.TargetExcludeColumns,
            RecordFilterIncludeOverride = m.RecordFilterIncludeOverride,
            RecordFilterExcludeOverride = m.RecordFilterExcludeOverride,
        }).ToList(),
        CreatedAt = def.CreatedAt,
        UpdatedAt = def.UpdatedAt,
    };
}
