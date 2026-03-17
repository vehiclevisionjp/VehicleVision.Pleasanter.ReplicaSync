using System.ComponentModel.DataAnnotations;
using ReplicaSync.Core.Enums;

namespace ReplicaSync.Web.Api.Models;

/// <summary>
/// Response model for a sync instance.
/// </summary>
public sealed class SyncInstanceResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the instance identifier.</summary>
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the RDBMS type.</summary>
    public DbmsType DbmsType { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request model for creating a sync instance.
/// </summary>
public sealed class CreateSyncInstanceRequest
{
    /// <summary>Gets or sets the instance identifier.</summary>
    [Required]
    [MaxLength(100)]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the RDBMS type.</summary>
    public DbmsType DbmsType { get; set; }

    /// <summary>Gets or sets the database connection string.</summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating a sync instance.
/// </summary>
public sealed class UpdateSyncInstanceRequest
{
    /// <summary>Gets or sets the instance identifier.</summary>
    [Required]
    [MaxLength(100)]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the RDBMS type.</summary>
    public DbmsType DbmsType { get; set; }

    /// <summary>Gets or sets the database connection string.</summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
