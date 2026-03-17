using System.Security.Cryptography;

namespace ReplicaSync.Web.Security;

/// <summary>
/// Provides PBKDF2-based password hashing and verification.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A string in the format "base64salt:base64hash".</returns>
    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="storedHash">The stored hash in "base64salt:base64hash" format.</param>
    /// <returns><c>true</c> if the password matches; otherwise, <c>false</c>.</returns>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
