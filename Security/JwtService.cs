using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarDealership.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarDealership.Api.Security;

/// <summary>
/// Configuration options for JWT token generation
/// </summary>
public class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Key { get; set; } = default!;
}

public interface IJwtService
{
    string CreateToken(User user);
}

/// <summary>
/// Service responsible for creating JWT tokens for authenticated users
/// Tokens contain user identity and role information for authorization
/// </summary>
public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwtConfiguration = jwtOptions.Value;

    /// <summary>
    /// Creates a JWT token containing user claims (ID, email, role, name)
    /// Token expires after 8 hours for security
    /// </summary>
    public string CreateToken(User user)
    {
        // Define user claims that will be embedded in the token
        var userClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("name", user.FullName ?? "")
        };

        // Create signing credentials using the secret key
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        
        // Create JWT token with claims and expiration
        var jwtToken = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer, 
            audience: _jwtConfiguration.Audience, 
            claims: userClaims,
            expires: DateTime.UtcNow.AddHours(8), 
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
