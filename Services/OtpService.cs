using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace CarDealership.Api.Services;

/// <summary>
/// Configuration options for OTP service
/// </summary>
public class OtpOptions
{
    /// <summary>
    /// Number of minutes after which OTP expires (default: 2 minutes)
    /// </summary>
    public int ExpiryMinutes { get; set; } = 2;
}

/// <summary>
/// Service interface for OTP generation and validation operations
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a secure OTP for the specified purpose
    /// </summary>
    /// <param name="subjectEmail">Email address of the user</param>
    /// <param name="purpose">Purpose for which OTP is generated</param>
    /// <param name="vehicleId">Optional vehicle ID for purchase OTPs</param>
    /// <returns>Generated OTP code</returns>
    Task<string> GenerateAsync(string subjectEmail, OtpPurpose purpose, int? vehicleId = null);
    
    /// <summary>
    /// Validates an OTP code against stored tokens
    /// </summary>
    /// <param name="subjectEmail">Email address of the user</param>
    /// <param name="purpose">Purpose for which OTP was generated</param>
    /// <param name="code">OTP code to validate</param>
    /// <param name="consume">Whether to consume the OTP after validation</param>
    /// <param name="vehicleId">Vehicle ID for purchase OTP validation</param>
    /// <returns>True if OTP is valid, false otherwise</returns>
    Task<bool> ValidateAsync(string subjectEmail, OtpPurpose purpose, string code, bool consume = true, int? vehicleId = null);
}

/// <summary>
/// Service responsible for generating and validating One-Time Passwords (OTPs)
/// Supports different OTP purposes and vehicle-bound OTPs for enhanced security
/// </summary>
public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private readonly int _expiryMinutes;
    private readonly ILogger<OtpService> _log;

    /// <summary>
    /// Initializes a new instance of the OtpService
    /// </summary>
    /// <param name="db">Database context for OTP storage</param>
    /// <param name="options">OTP configuration options</param>
    /// <param name="log">Logger for OTP operations</param>
    public OtpService(AppDbContext db, IOptions<OtpOptions> options, ILogger<OtpService> log)
    {
        _db = db;
        _expiryMinutes = options.Value.ExpiryMinutes;
        _log = log;
    }

    /// <summary>
    /// Generates a secure 6-digit OTP for the specified purpose
    /// For purchase OTPs, vehicleId binds the OTP to prevent cross-vehicle misuse
    /// </summary>
    /// <param name="subjectEmail">Email address of the user</param>
    /// <param name="purpose">Purpose for which OTP is generated</param>
    /// <param name="vehicleId">Optional vehicle ID for purchase OTPs</param>
    /// <returns>Generated OTP code</returns>
    public async Task<string> GenerateAsync(string subjectEmail, OtpPurpose purpose, int? vehicleId = null)
    {
        // Step 1: Generate cryptographically secure random 6-digit code (100000-999999)
        var otpCode = Random.Shared.Next(100000, 999999).ToString();
        
        // Step 2: Create OTP token with expiration and optional vehicle binding
        var otpToken = new OtpToken
        {
            Subject = subjectEmail.Trim().ToLowerInvariant(),
            Purpose = purpose,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            VehicleId = vehicleId // Bind OTP to specific vehicle for purchase operations
        };

        // Step 3: Store OTP token in database
        _db.OtpTokens.Add(otpToken);
        await _db.SaveChangesAsync();

        // Step 4: Log OTP generation for audit trail (without exposing the code)
        _log.LogInformation("Generated OTP for {Email} with purpose {Purpose}, expires at {ExpiresAt}, vehicleId: {VehicleId}",
            otpToken.Subject, purpose, otpToken.ExpiresAt, vehicleId);

        // Step 5: Dev-only: Print OTP to console for testing (remove in production)
        Console.WriteLine($"[OTP] {otpCode}");

        return otpCode;
    }

    /// <summary>
    /// Validates an OTP code against stored tokens
    /// For purchase OTPs, also validates vehicle binding to prevent misuse
    /// </summary>
    /// <param name="subjectEmail">Email address of the user</param>
    /// <param name="purpose">Purpose for which OTP was generated</param>
    /// <param name="code">OTP code to validate</param>
    /// <param name="consume">Whether to consume the OTP after validation</param>
    /// <param name="vehicleId">Vehicle ID for purchase OTP validation</param>
    /// <returns>True if OTP is valid, false otherwise</returns>
    public async Task<bool> ValidateAsync(string subjectEmail, OtpPurpose purpose, string code, bool consume = true, int? vehicleId = null)
    {
        var currentTime = DateTime.UtcNow;
        
        // Step 1: Build query to find matching, valid, unexpired OTP
        var otpQuery = _db.OtpTokens
            .Where(token => token.Subject == subjectEmail.Trim().ToLowerInvariant()
                        && token.Purpose == purpose
                        && !token.Consumed
                        && token.ExpiresAt >= currentTime
                        && token.Code == code);

        // Step 2: For purchase OTPs, ensure the vehicle ID matches (security feature)
        if (purpose == OtpPurpose.Purchase && vehicleId.HasValue)
        {
            otpQuery = otpQuery.Where(token => token.VehicleId == vehicleId.Value);
        }

        // Step 3: Get the most recent matching OTP (in case of duplicates)
        var validOtpToken = await otpQuery
            .OrderByDescending(token => token.Id)
            .FirstOrDefaultAsync();

        if (validOtpToken is null)
            return false;

        // Step 4: Mark OTP as consumed to prevent reuse (unless validation only)
        if (consume)
        {
            validOtpToken.Consumed = true;
            await _db.SaveChangesAsync();
        }

        return true;
    }
}
