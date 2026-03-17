namespace VehicleVision.Pleasanter.ReplicaSync.Web.Security;

/// <summary>
/// Security settings for the ReplicaSync web application.
/// </summary>
public sealed class SecuritySettings
{
    /// <summary>Gets or sets the configured API keys.</summary>
    public List<ApiKeyEntry> ApiKeys { get; set; } = [];

    /// <summary>Gets or sets the IP whitelist settings.</summary>
    public IpWhitelistSettings IpWhitelist { get; set; } = new();

    /// <summary>Gets or sets the account lockout settings.</summary>
    public AccountLockoutSettings AccountLockout { get; set; } = new();

    /// <summary>Gets or sets the initial admin username (for first-run seeding only).</summary>
    public string InitialAdminUsername { get; set; } = "administrator";

    /// <summary>Gets or sets the initial admin password (for first-run seeding only).</summary>
    public string InitialAdminPassword { get; set; } = "vehiclevision";
}

/// <summary>
/// Represents an API key entry.
/// </summary>
public sealed class ApiKeyEntry
{
    /// <summary>Gets or sets the display name for this API key.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the API key value.</summary>
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Account lockout configuration for brute-force protection.
/// </summary>
public sealed class AccountLockoutSettings
{
    /// <summary>Gets or sets whether account lockout is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the number of consecutive failed login attempts before lockout.</summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>Gets or sets the lockout duration in minutes.</summary>
    public int LockoutDurationMinutes { get; set; } = 15;
}

/// <summary>
/// IP whitelist configuration.
/// </summary>
public sealed class IpWhitelistSettings
{
    /// <summary>Gets or sets whether IP filtering is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the allowed IP addresses or CIDR ranges.</summary>
    public List<string> AllowedAddresses { get; set; } = [];
}
