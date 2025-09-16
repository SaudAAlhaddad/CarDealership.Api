using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Api.Security;

// This service turns passwords into SHA256 hashe to not store the passwords as plain text in the database
public static class PasswordHasher
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
