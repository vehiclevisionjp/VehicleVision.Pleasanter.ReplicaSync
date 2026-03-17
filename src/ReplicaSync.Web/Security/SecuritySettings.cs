namespace ReplicaSync.Web.Security;

/// <summary>
/// Security settings for the ReplicaSync web application.
/// </summary>
public sealed class SecuritySettings
{
    /// <summary>Gets or sets the configured API keys.</summary>
    public List<ApiKeyEntry> ApiKeys { get; set; } = [];

    /// <summary>Gets or sets the IP whitelist settings.</summary>
    public IpWhitelistSettings IpWhitelist { get; set; } = new();

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
/// IP whitelist configuration.
/// </summary>
public sealed class IpWhitelistSettings
{
    /// <summary>Gets or sets whether IP filtering is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the allowed IP addresses or CIDR ranges.</summary>
    public List<string> AllowedAddresses { get; set; } = [];
}
