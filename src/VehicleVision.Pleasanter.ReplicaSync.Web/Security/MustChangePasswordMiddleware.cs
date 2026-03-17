namespace VehicleVision.Pleasanter.ReplicaSync.Web.Security;

/// <summary>
/// Middleware that redirects authenticated users who must change their password
/// to the password change page.
/// </summary>
public sealed class MustChangePasswordMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="MustChangePasswordMiddleware"/> class.
    /// </summary>
    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request and redirects if password change is required.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var mustChange = context.User.FindFirst("MustChangePassword");
            if (mustChange is not null
                && bool.TryParse(mustChange.Value, out var required)
                && required)
            {
                var path = context.Request.Path.Value ?? string.Empty;

                // Allow access to change-password, logout, and static assets
                if (!path.StartsWith("/account/change-password", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/account/logout", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect("/account/change-password");
                    return;
                }
            }
        }

        await _next(context).ConfigureAwait(false);
    }
}
