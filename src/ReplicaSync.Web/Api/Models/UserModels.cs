using System.ComponentModel.DataAnnotations;
using ReplicaSync.Core.Enums;

namespace ReplicaSync.Web.Api.Models;

/// <summary>
/// Response model for a user.
/// </summary>
public sealed class UserResponse
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the login username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's role.</summary>
    public AppRole Role { get; set; }

    /// <summary>Gets or sets whether the user must change their password.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request model for creating a user.
/// </summary>
public sealed class CreateUserRequest
{
    /// <summary>Gets or sets the login username.</summary>
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the plain-text password.</summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's role.</summary>
    public AppRole Role { get; set; } = AppRole.User;

    /// <summary>Gets or sets whether the user must change their password.</summary>
    public bool MustChangePassword { get; set; } = true;
}

/// <summary>
/// Request model for updating a user.
/// </summary>
public sealed class UpdateUserRequest
{
    /// <summary>Gets or sets the login username.</summary>
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's role.</summary>
    public AppRole Role { get; set; } = AppRole.User;

    /// <summary>Gets or sets whether the user must change their password.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Gets or sets the new password (optional; null means no password change).</summary>
    [MinLength(8)]
    public string? Password { get; set; }
}
