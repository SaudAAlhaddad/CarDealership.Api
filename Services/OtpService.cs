using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CarDealership.Api.Services;

public class OtpOptions
{
    public int ExpiryMinutes { get; set; } = 2;
}

public interface IOtpService
{
    Task<string> GenerateAsync(string subjectEmail, OtpPurpose purpose);
    Task<bool> ValidateAsync(string subjectEmail, OtpPurpose purpose, string code, bool consume = true);
}

public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private readonly int _expiryMinutes;

    public OtpService(AppDbContext db, IOptions<OtpOptions> options)
    {
        _db = db;
        _expiryMinutes = options.Value.ExpiryMinutes;

        Console.WriteLine($"[OtpService] OTP expiry = {_expiryMinutes} minutes");
    }

    public async Task<string> GenerateAsync(string subjectEmail, OtpPurpose purpose)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var token = new OtpToken
        {
            Subject = subjectEmail.Trim().ToLowerInvariant(),
            Purpose = purpose,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes)
        };

        _db.OtpTokens.Add(token);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[OTP] To={token.Subject} Purpose={purpose} Code={code} ExpiresAt={token.ExpiresAt:o}");

        return code;
    }

    public async Task<bool> ValidateAsync(string subjectEmail, OtpPurpose purpose, string code, bool consume = true)
    {
        var now = DateTime.UtcNow;
        var token = await _db.OtpTokens
            .Where(t => t.Subject == subjectEmail.Trim().ToLowerInvariant()
                        && t.Purpose == purpose
                        && !t.Consumed
                        && t.ExpiresAt >= now
                        && t.Code == code)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();

        if (token is null)
            return false;

        if (consume)
        {
            token.Consumed = true;
            await _db.SaveChangesAsync();
        }

        return true;
    }
}
