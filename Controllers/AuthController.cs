using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Security;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Controllers;

// This controller handles auth-related endpoints under /api/auth
// It supports: requesting OTP, registering a new customer, logging in 

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IOtpService otp, IJwtService jwt, ILogger<AuthController> log) : ControllerBase
{
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");
        if (!Enum.IsDefined(dto.Purpose)) return BadRequest("Invalid purpose.");

        await otp.GenerateAsync(dto.Email, dto.Purpose);
        return Ok(new { message = "OTP generated. Check server console output." });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!await otp.ValidateAsync(dto.Email, OtpPurpose.Register, dto.OtpCode))
            return BadRequest("Invalid or expired OTP.");

        if (await db.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
            return Conflict("Email already registered.");

        var user = new User
        {
            Email = dto.Email.ToLower(),
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Role = UserRole.Customer,
            FullName = dto.FullName
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = jwt.CreateToken(user);
        return Ok(new AuthResultDto(token, user.Role.ToString(), user.Email, user.FullName ?? ""));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email.ToLower());
        if (user is null || user.PasswordHash != PasswordHasher.Hash(dto.Password))
            return Unauthorized("Invalid credentials.");

        if (!await otp.ValidateAsync(dto.Email, OtpPurpose.Login, dto.OtpCode))
            return BadRequest("Invalid or expired OTP.");

        var token = jwt.CreateToken(user);
        return Ok(new AuthResultDto(token, user.Role.ToString(), user.Email, user.FullName ?? ""));
    }
}
