using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ReplicaSync.Web.Security;

namespace ReplicaSync.Web.Tests.Security;

public class IpWhitelistMiddlewareTests
{
    private static IpWhitelistMiddleware CreateMiddleware(
        IpWhitelistSettings settings,
        RequestDelegate? next = null)
    {
        next ??= _ => Task.CompletedTask;
        var securitySettings = new SecuritySettings { IpWhitelist = settings };
        var options = Options.Create(securitySettings);
        var logger = NullLogger<IpWhitelistMiddleware>.Instance;
        return new IpWhitelistMiddleware(next, logger, options);
    }

    private static DefaultHttpContext CreateContext(string ipAddress)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        return context;
    }

    [Fact]
    public async Task DisabledShouldAllowAllRequests()
    {
        // Arrange
        var settings = new IpWhitelistSettings { Enabled = false, AllowedAddresses = [] };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task EnabledShouldAllowWhitelistedIp()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["192.168.1.100"]
        };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task EnabledShouldBlockNonWhitelistedIp()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["192.168.1.1"]
        };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("10.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task CidrShouldAllowAddressInRange()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["192.168.1.0/24"]
        };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("192.168.1.50");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task CidrShouldBlockAddressOutsideRange()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["192.168.1.0/24"]
        };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("192.168.2.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task LoopbackShouldBeAllowedWhenConfigured()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["127.0.0.1", "::1"]
        };
        var middleware = CreateMiddleware(settings);
        var context = CreateContext("127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.NotEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task NullRemoteIpShouldBeBlocked()
    {
        // Arrange
        var settings = new IpWhitelistSettings
        {
            Enabled = true,
            AllowedAddresses = ["127.0.0.1"]
        };
        var middleware = CreateMiddleware(settings);
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }
}
