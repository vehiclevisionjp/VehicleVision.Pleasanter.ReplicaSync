using System.ComponentModel.DataAnnotations;
using ReplicaSync.Core.Enums;

namespace ReplicaSync.Core.Models;

/// <summary>
/// Represents a Pleasanter instance with its database connection information.
/// </summary>
public class SyncInstance
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the instance identifier (e.g., "headquarters", "branch-a").</summary>
    [Required]
    [MaxLength(100)]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets a display name for this instance.</summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the RDBMS type.</summary>
    public DbmsType DbmsType { get; set; }

    /// <summary>Gets or sets the database connection string.</summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
