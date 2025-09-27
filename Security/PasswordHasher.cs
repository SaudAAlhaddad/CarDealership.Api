using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Api.Security;

/// <summary>
/// Static utility class for password hashing using SHA256
/// Converts plain text passwords to irreversible hashes for secure storage
/// Note: SHA256 is used here for simplicity, but bcrypt/Argon2 are recommended for production
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Creates a SHA256 hash of the input password
    /// </summary>
    /// <param name="plainTextPassword">The password to hash</param>
    /// <returns>Hexadecimal string representation of the hash</returns>
    public static string Hash(string plainTextPassword)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainTextPassword));
        return Convert.ToHexString(passwordBytes);
    }
}
