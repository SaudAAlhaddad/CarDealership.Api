using CarDealership.Api.Entities;

namespace CarDealership.Api.Dtos;

public record RequestOtpDto(string Email, OtpPurpose Purpose);
public record RequestAuthenticatedOtpDto(OtpPurpose Purpose); // For authenticated users
public record RequestPurchaseOtpDto(int VehicleId); // Email comes from JWT token
public record RegisterDto(string Email, string Password, string FullName, string OtpCode);
public record LoginDto(string Email, string Password, string OtpCode);
public record AuthResultDto(string Token, string Role, string Email, string FullName);
