using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Security;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;

/// <summary>
/// API controller for managing sync instances.
/// </summary>
[ApiController]
[Route("api/sync-instances")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public sealed class SyncInstancesController : ControllerBase
{
    private readonly ISyncConfigRepository _syncRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncInstancesController"/> class.
    /// </summary>
    public SyncInstancesController(ISyncConfigRepository syncRepo)
    {
        _syncRepo = syncRepo;
    }

    /// <summary>Gets all sync instances.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SyncInstanceResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var instances = await _syncRepo.GetAllInstancesAsync(cancellationToken);
        return Ok(instances.Select(ToResponse));
    }

    /// <summary>Gets a sync instance by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SyncInstanceResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var instance = await _syncRepo.GetInstanceByIdAsync(id, cancellationToken);
        if (instance is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(instance));
    }

    /// <summary>Creates a new sync instance.</summary>
    [HttpPost]
    public async Task<ActionResult<SyncInstanceResponse>> CreateAsync(
        [FromBody] CreateSyncInstanceRequest request,
        CancellationToken cancellationToken)
    {
        var instance = new SyncInstance
        {
            InstanceId = request.InstanceId,
            DisplayName = request.DisplayName,
            DbmsType = request.DbmsType,
            ConnectionString = request.ConnectionString,
        };

        var created = await _syncRepo.CreateInstanceAsync(instance, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, ToResponse(created));
    }

    /// <summary>Updates an existing sync instance.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SyncInstanceResponse>> UpdateAsync(
        int id,
        [FromBody] UpdateSyncInstanceRequest request,
        CancellationToken cancellationToken)
    {
        var instance = await _syncRepo.GetInstanceByIdAsync(id, cancellationToken);
        if (instance is null)
        {
            return NotFound();
        }

        instance.InstanceId = request.InstanceId;
        instance.DisplayName = request.DisplayName;
        instance.DbmsType = request.DbmsType;
        instance.ConnectionString = request.ConnectionString;

        var updated = await _syncRepo.UpdateInstanceAsync(instance, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Deletes a sync instance by ID.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var instance = await _syncRepo.GetInstanceByIdAsync(id, cancellationToken);
        if (instance is null)
        {
            return NotFound();
        }

        await _syncRepo.DeleteInstanceAsync(id, cancellationToken);
        return NoContent();
    }

    private static SyncInstanceResponse ToResponse(SyncInstance instance) => new()
    {
        Id = instance.Id,
        InstanceId = instance.InstanceId,
        DisplayName = instance.DisplayName,
        DbmsType = instance.DbmsType,
        CreatedAt = instance.CreatedAt,
        UpdatedAt = instance.UpdatedAt,
    };
}
