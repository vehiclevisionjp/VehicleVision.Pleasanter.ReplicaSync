using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using VehicleVision.Pleasanter.ReplicaSync.Web.Security;

namespace VehicleVision.Pleasanter.ReplicaSync.Web.Tests.Security;

public class MustChangePasswordMiddlewareTests
{
    private static MustChangePasswordMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new MustChangePasswordMiddleware(next);
    }

    private static DefaultHttpContext CreateAuthenticatedContext(bool mustChangePassword)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new("MustChangePassword", mustChangePassword.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity),
        };
        context.Request.Path = "/";
        return context;
    }

    [Fact]
    public async Task ShouldRedirectWhenMustChangePasswordIsTrue()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateAuthenticatedContext(mustChangePassword: true);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.Equal("/account/change-password", context.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task ShouldNotRedirectWhenMustChangePasswordIsFalse()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateAuthenticatedContext(mustChangePassword: false);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ShouldAllowAccessToChangePasswordPage()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateAuthenticatedContext(mustChangePassword: true);
        context.Request.Path = "/account/change-password";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ShouldAllowAccessToLogoutPage()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = CreateAuthenticatedContext(mustChangePassword: true);
        context.Request.Path = "/account/logout";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ShouldPassThroughForUnauthenticatedUsers()
    {
        // Arrange
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
