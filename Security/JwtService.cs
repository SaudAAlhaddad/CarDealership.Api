using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarDealership.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarDealership.Api.Security;

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

// This service creates tokens for users who are logged in
public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _opt = options.Value;

    public string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("name", user.FullName ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_opt.Issuer, _opt.Audience, claims,
            expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
