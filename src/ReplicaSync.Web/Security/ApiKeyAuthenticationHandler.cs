using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ReplicaSync.Web.Security;

/// <summary>
/// Authentication handler that validates API keys from the X-Api-Key header.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The authentication scheme name.</summary>
    public const string SchemeName = "ApiKey";

    /// <summary>The HTTP header name for API key.</summary>
    public const string HeaderName = "X-Api-Key";

    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
    /// </summary>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<SecuritySettings> securitySettings)
        : base(options, logger, encoder)
    {
        _securitySettings = securitySettings.Value;
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var apiKeyValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedKey = apiKeyValues.ToString();
        if (string.IsNullOrEmpty(providedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var matchedKey = _securitySettings.ApiKeys.Find(k =>
            string.Equals(k.Key, providedKey, StringComparison.Ordinal));

        if (matchedKey is null)
        {
            Logger.LogWarning("ApiKey認証: 無効なAPIキーが使用されました。");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, matchedKey.Name),
            new Claim(ClaimTypes.AuthenticationMethod, SchemeName),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        Logger.LogInformation("ApiKey認証: {ApiKeyName} が認証されました。", matchedKey.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
