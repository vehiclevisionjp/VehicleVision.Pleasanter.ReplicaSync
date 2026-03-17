using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;
using VehicleVision.Pleasanter.ReplicaSync.Core.Models;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Tests.Models;

public class AppUserTests
{
    [Fact]
    public void DefaultRoleShouldBeUser()
    {
        // Arrange & Act
        var user = new AppUser();

        // Assert
        Assert.Equal(AppRole.User, user.Role);
    }

    [Fact]
    public void RoleShouldBeSettableToAdministrator()
    {
        // Arrange
        var user = new AppUser();

        // Act
        user.Role = AppRole.Administrator;

        // Assert
        Assert.Equal(AppRole.Administrator, user.Role);
    }

    [Fact]
    public void DefaultPropertiesShouldBeInitialized()
    {
        // Arrange & Act
        var user = new AppUser();

        // Assert
        Assert.Equal(0, user.Id);
        Assert.Equal(string.Empty, user.Username);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Equal(AppRole.User, user.Role);
        Assert.False(user.MustChangePassword);
        Assert.Equal(0, user.FailedLoginCount);
        Assert.Null(user.LockoutEndUtc);
        Assert.False(user.IsLockedOut);
    }

    [Fact]
    public void IsLockedOutShouldReturnTrueWhenLockoutEndIsInFuture()
    {
        // Arrange
        var user = new AppUser { LockoutEndUtc = DateTime.UtcNow.AddMinutes(10) };

        // Act & Assert
        Assert.True(user.IsLockedOut);
    }

    [Fact]
    public void IsLockedOutShouldReturnFalseWhenLockoutEndIsInPast()
    {
        // Arrange
        var user = new AppUser { LockoutEndUtc = DateTime.UtcNow.AddMinutes(-1) };

        // Act & Assert
        Assert.False(user.IsLockedOut);
    }

    [Fact]
    public void IsLockedOutShouldReturnFalseWhenLockoutEndIsNull()
    {
        // Arrange
        var user = new AppUser { LockoutEndUtc = null };

        // Act & Assert
        Assert.False(user.IsLockedOut);
    }
}
