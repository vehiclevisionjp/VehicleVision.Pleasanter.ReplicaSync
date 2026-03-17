using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleVision.Pleasanter.ReplicaSync.Core.Interfaces;
using VehicleVision.Pleasanter.ReplicaSync.Web.Api.Models;
using VehicleVision.Pleasanter.ReplicaSync.Web.Security;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Api.Controllers;

/// <summary>
/// API controller for viewing sync log entries.
/// </summary>
[ApiController]
[Route("api/sync-logs")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
public sealed class SyncLogsController : ControllerBase
{
    private readonly ISyncConfigRepository _syncRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncLogsController"/> class.
    /// </summary>
    public SyncLogsController(ISyncConfigRepository syncRepo)
    {
        _syncRepo = syncRepo;
    }

    /// <summary>Gets recent sync log entries.</summary>
    /// <param name="count">Number of entries to return (default: 100, max: 1000).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SyncLogEntryResponse>>> GetRecentAsync(
        [FromQuery] int count = 100,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 1000)
        {
            return BadRequest(new { message = "count は 1〜1000 の範囲で指定してください。" });
        }

        var logs = await _syncRepo.GetRecentLogsAsync(count, cancellationToken);
        var response = logs.Select(log => new SyncLogEntryResponse
        {
            Id = log.Id,
            SyncId = log.SyncId,
            SourceInstanceId = log.SourceInstanceId,
            TargetInstanceId = log.TargetInstanceId,
            Status = log.Status,
            RecordsProcessed = log.RecordsProcessed,
            RecordsInserted = log.RecordsInserted,
            RecordsUpdated = log.RecordsUpdated,
            RecordsDeleted = log.RecordsDeleted,
            ConflictsDetected = log.ConflictsDetected,
            ErrorMessage = log.ErrorMessage,
            StartedAt = log.StartedAt,
            CompletedAt = log.CompletedAt,
        });

        return Ok(response);
    }
}
