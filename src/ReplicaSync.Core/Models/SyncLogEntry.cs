using System.ComponentModel.DataAnnotations;
using ReplicaSync.Core.Enums;

namespace ReplicaSync.Core.Models;

/// <summary>
/// Represents a log entry for a sync operation.
/// </summary>
public class SyncLogEntry
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the sync definition ID.</summary>
    [Required]
    [MaxLength(100)]
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the source instance ID.</summary>
    [MaxLength(100)]
    public string SourceInstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the target instance ID.</summary>
    [MaxLength(100)]
    public string TargetInstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the status of this sync operation.</summary>
    public SyncStatus Status { get; set; }

    /// <summary>Gets or sets the number of records processed.</summary>
    public int RecordsProcessed { get; set; }

    /// <summary>Gets or sets the number of records inserted.</summary>
    public int RecordsInserted { get; set; }

    /// <summary>Gets or sets the number of records updated.</summary>
    public int RecordsUpdated { get; set; }

    /// <summary>Gets or sets the number of records deleted.</summary>
    public int RecordsDeleted { get; set; }

    /// <summary>Gets or sets the number of conflicts detected.</summary>
    public int ConflictsDetected { get; set; }

    /// <summary>Gets or sets the error message if the sync failed.</summary>
    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets when the sync operation started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets when the sync operation completed.</summary>
    public DateTime? CompletedAt { get; set; }
}
