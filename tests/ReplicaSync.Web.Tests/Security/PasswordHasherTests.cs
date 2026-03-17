using ReplicaSync.Web.Security;

namespace ReplicaSync.Web.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void HashPasswordShouldReturnSaltColonHashFormat()
    {
        // Arrange & Act
        var result = PasswordHasher.HashPassword("test-password");

        // Assert
        Assert.Contains(':', result);
        var parts = result.Split(':');
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrEmpty(parts[0]));
        Assert.False(string.IsNullOrEmpty(parts[1]));
    }

    [Fact]
    public void HashPasswordShouldProduceDifferentHashesForSamePassword()
    {
        // Arrange & Act
        var hash1 = PasswordHasher.HashPassword("same-password");
        var hash2 = PasswordHasher.HashPassword("same-password");

        // Assert (different salts produce different hashes)
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPasswordShouldReturnTrueForCorrectPassword()
    {
        // Arrange
        var hash = PasswordHasher.HashPassword("correct-password");

        // Act & Assert
        Assert.True(PasswordHasher.VerifyPassword("correct-password", hash));
    }

    [Fact]
    public void VerifyPasswordShouldReturnFalseForWrongPassword()
    {
        // Arrange
        var hash = PasswordHasher.HashPassword("correct-password");

        // Act & Assert
        Assert.False(PasswordHasher.VerifyPassword("wrong-password", hash));
    }

    [Fact]
    public void VerifyPasswordShouldReturnFalseForEmptyPassword()
    {
        // Arrange
        var hash = PasswordHasher.HashPassword("some-password");

        // Act & Assert
        Assert.False(PasswordHasher.VerifyPassword("", hash));
    }

    [Fact]
    public void VerifyPasswordShouldReturnFalseForEmptyHash()
    {
        // Act & Assert
        Assert.False(PasswordHasher.VerifyPassword("password", ""));
    }

    [Fact]
    public void VerifyPasswordShouldReturnFalseForInvalidHashFormat()
    {
        // Act & Assert
        Assert.False(PasswordHasher.VerifyPassword("password", "not-a-valid-hash"));
    }

    [Fact]
    public void VerifyPasswordShouldReturnFalseForInvalidBase64()
    {
        // Act & Assert
        Assert.False(PasswordHasher.VerifyPassword("password", "!!!:!!!"));
    }

    [Fact]
    public void HashPasswordShouldThrowForNullPassword()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PasswordHasher.HashPassword(null!));
    }

    [Fact]
    public void HashPasswordShouldThrowForEmptyPassword()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PasswordHasher.HashPassword(""));
    }
}
