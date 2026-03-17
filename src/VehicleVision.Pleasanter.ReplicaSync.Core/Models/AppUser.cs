using System.ComponentModel.DataAnnotations;
using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Models;

/// <summary>
/// Represents an application user for management web authentication.
/// </summary>
public class AppUser
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the login username.</summary>
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the PBKDF2 password hash (format: "base64salt:base64hash").</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's role.</summary>
    public AppRole Role { get; set; } = AppRole.User;

    /// <summary>Gets or sets whether the user must change their password at next login.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>Gets or sets the number of consecutive failed login attempts.</summary>
    public int FailedLoginCount { get; set; }

    /// <summary>Gets or sets the UTC time until which the account is locked out. Null if not locked.</summary>
    public DateTime? LockoutEndUtc { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Returns whether the account is currently locked out.
    /// </summary>
    public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
}
