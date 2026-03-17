using System.ComponentModel.DataAnnotations;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Models;

/// <summary>
/// Represents a version history snapshot of a Pleasanter record captured before a sync overwrite.
/// </summary>
public class RecordVersionHistory
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the sync definition ID that triggered this snapshot.</summary>
    [Required]
    [MaxLength(100)]
    public string SyncId { get; set; } = string.Empty;

    /// <summary>Gets or sets the instance ID where this snapshot was captured.</summary>
    [Required]
    [MaxLength(100)]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the Pleasanter site ID.</summary>
    public long SiteId { get; set; }

    /// <summary>Gets or sets the Pleasanter record ID (ResultId or IssueId).</summary>
    public long RecordId { get; set; }

    /// <summary>Gets or sets the sequential version number for this record's history (1-based).</summary>
    public int VersionNumber { get; set; }

    /// <summary>Gets or sets the record title at the time of snapshot.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the record body at the time of snapshot.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the column values as a JSON-serialized snapshot.</summary>
    public string ColumnSnapshotJson { get; set; } = "{}";

    /// <summary>Gets or sets the Pleasanter user ID who last edited the record before this snapshot.</summary>
    public int ChangedBy { get; set; }

    /// <summary>Gets or sets the Pleasanter UpdatedTime of the record at snapshot time.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>Gets or sets whether this snapshot was captured before a deletion (true) or an update (false).</summary>
    public bool IsDeleteSnapshot { get; set; }

    /// <summary>Gets or sets when this version history entry was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
