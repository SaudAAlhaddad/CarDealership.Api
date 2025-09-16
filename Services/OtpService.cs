using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

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

// This handles creating and checking OTPs
public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private readonly int _expiryMinutes;
    private readonly ILogger<OtpService> _log;

    public OtpService(AppDbContext db, IOptions<OtpOptions> options, ILogger<OtpService> log)
    {
        _db = db;
        _expiryMinutes = options.Value.ExpiryMinutes;
        _log = log;

        _log.LogInformation("OTP expiry configured to {ExpiryMinutes} minutes", _expiryMinutes);
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

        // Structured log (metadata only)
        _log.LogInformation("Generated OTP for {Email} with purpose {Purpose}, expires at {ExpiresAt}",
            token.Subject, purpose, token.ExpiresAt);

        // Dev-only OTP code print
        Console.WriteLine($"[OTP] {code}");

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
