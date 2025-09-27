using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarDealership.Api.Controllers;

// This controller handles auth-related endpoints under /api/auth
// It supports: requesting OTP, registering a new customer, logging in 

[ApiController]
[Route("api/auth")]
public class AuthController(IOtpService otpService, IUserService userService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Generates OTP for unauthenticated users (registration/login only)
    /// Purchase OTPs must use the dedicated vehicle-bound endpoint
    /// </summary>
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto otpRequest)
    {
        if (string.IsNullOrWhiteSpace(otpRequest.Email)) return BadRequest("Email is required.");
        if (!Enum.IsDefined(otpRequest.Purpose)) return BadRequest("Invalid purpose.");
        
        // Purchase OTPs must use the dedicated endpoint for vehicle binding
        if (otpRequest.Purpose == OtpPurpose.Purchase)
            return BadRequest("Use /api/auth/request-purchase-otp endpoint for purchase OTPs.");

        await otpService.GenerateAsync(otpRequest.Email, otpRequest.Purpose);
        return Ok(new { message = "OTP generated. Check server console output." });
    }

    /// <summary>
    /// Generates vehicle-bound purchase OTP for authenticated customers
    /// OTP is cryptographically bound to the specific vehicle to prevent misuse
    /// </summary>
    [HttpPost("request-purchase-otp")]
    [Authorize]
    public async Task<IActionResult> RequestPurchaseOtp([FromBody] RequestPurchaseOtpDto purchaseOtpRequest)
    {
        // Extract customer email from JWT token (no need to send it in request)
        var customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(customerEmail)) return Unauthorized("Email not found in token.");
        if (purchaseOtpRequest.VehicleId <= 0) return BadRequest("Valid vehicle ID is required.");

        // Generate OTP bound to specific vehicle for security
        await otpService.GenerateAsync(customerEmail, OtpPurpose.Purchase, purchaseOtpRequest.VehicleId);
        return Ok(new { message = "Purchase OTP generated. Check server console output." });
    }

    /// <summary>
    /// Generates OTP for authenticated users (UpdateVehicle, etc.)
    /// Email is extracted from JWT token to prevent spoofing
    /// </summary>
    [HttpPost("request-authenticated-otp")]
    [Authorize]
    public async Task<IActionResult> RequestAuthenticatedOtp([FromBody] RequestAuthenticatedOtpDto authenticatedOtpRequest)
    {
        // Extract user email from JWT token 
        var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(userEmail)) return Unauthorized("Email not found in token.");
        if (!Enum.IsDefined(authenticatedOtpRequest.Purpose)) return BadRequest("Invalid purpose.");
        
        // Purchase OTPs must use the dedicated endpoint for vehicle binding
        if (authenticatedOtpRequest.Purpose == OtpPurpose.Purchase)
            return BadRequest("Use /api/auth/request-purchase-otp endpoint for purchase OTPs.");

        await otpService.GenerateAsync(userEmail, authenticatedOtpRequest.Purpose);
        return Ok(new { message = "OTP generated. Check server console output." });
    }

    /// <summary>
    /// Registers a new customer with OTP validation
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registrationData)
    {
        try
        {
            var authResult = await userService.RegisterAsync(registrationData);
            return Ok(authResult);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Authenticates existing user with password and OTP validation
    /// Returns JWT token for subsequent requests
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginCredentials)
    {
        try
        {
            var authResult = await userService.LoginAsync(loginCredentials);
            return Ok(authResult);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
