using System.Net;
using Microsoft.Extensions.Options;

namespace ReplicaSync.Web.Security;

/// <summary>
/// Middleware that restricts access based on client IP address.
/// </summary>
public sealed class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly IpWhitelistSettings _settings;
    private readonly List<(IPAddress Network, int PrefixLength)> _allowedRanges;

    /// <summary>
    /// Initializes a new instance of the <see cref="IpWhitelistMiddleware"/> class.
    /// </summary>
    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        IOptions<SecuritySettings> settings)
    {
        _next = next;
        _logger = logger;
        _settings = settings.Value.IpWhitelist;
        _allowedRanges = ParseAllowedAddresses(_settings.AllowedAddresses);
    }

    /// <summary>
    /// Processes the HTTP request and checks the client IP against the whitelist.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.Enabled)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is null)
        {
            _logger.LogWarning("IP制限: リモートIPアドレスを取得できません。リクエストを拒否します。");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        // Map IPv4-mapped IPv6 to IPv4 for consistent comparison
        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        if (!IsAllowed(remoteIp))
        {
            _logger.LogWarning("IP制限: {RemoteIp} からのアクセスが拒否されました。", remoteIp);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private bool IsAllowed(IPAddress remoteIp)
    {
        foreach (var (network, prefixLength) in _allowedRanges)
        {
            if (IsInRange(remoteIp, network, prefixLength))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInRange(IPAddress address, IPAddress network, int prefixLength)
    {
        // Exact match (prefixLength covers all bits)
        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();

        if (addressBytes.Length != networkBytes.Length)
        {
            return false;
        }

        int fullBytes = prefixLength / 8;
        int remainingBits = prefixLength % 8;

        for (int i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
            {
                return false;
            }
        }

        if (remainingBits > 0 && fullBytes < addressBytes.Length)
        {
            int mask = 0xFF << (8 - remainingBits);
            if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
            {
                return false;
            }
        }

        return true;
    }

    private static List<(IPAddress Network, int PrefixLength)> ParseAllowedAddresses(List<string> addresses)
    {
        var result = new List<(IPAddress, int)>();

        foreach (var entry in addresses)
        {
            var parts = entry.Split('/');
            if (IPAddress.TryParse(parts[0], out var ip))
            {
                int prefixLength = parts.Length > 1 && int.TryParse(parts[1], out var p)
                    ? p
                    : ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;

                result.Add((ip, prefixLength));
            }
        }

        return result;
    }
}
