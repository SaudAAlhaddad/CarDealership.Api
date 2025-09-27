namespace CarDealership.Api.Dtos;

/// <summary>
/// Data transfer object for customer purchase requests
/// Vehicle ID identifies the car to purchase, OTP provides security validation
/// </summary>
/// <param name="VehicleId">ID of the vehicle to purchase</param>
/// <param name="OtpCode">One-time password for purchase authorization</param>
public record PurchaseRequestDto(int VehicleId, string OtpCode);
