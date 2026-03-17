using VehicleVision.Pleasanter.ReplicaSync.Core.Enums;

namespace VehicleVision.Pleasanter.ReplicaSync.Core.Tests.Enums;

public class AppRoleTests
{
    [Fact]
    public void AppRoleShouldHaveUserValue()
    {
        // Assert
        Assert.Equal(0, (int)AppRole.User);
    }

    [Fact]
    public void AppRoleShouldHaveAdministratorValue()
    {
        // Assert
        Assert.Equal(1, (int)AppRole.Administrator);
    }

    [Fact]
    public void AppRoleShouldHaveExactlyTwoValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<AppRole>();

        // Assert
        Assert.Equal(2, values.Length);
    }

    [Theory]
    [InlineData("User", AppRole.User)]
    [InlineData("Administrator", AppRole.Administrator)]
    public void AppRoleShouldParseFromString(string input, AppRole expected)
    {
        // Act
        var result = Enum.Parse<AppRole>(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
