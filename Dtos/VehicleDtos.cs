namespace CarDealership.Api.Dtos;

/// <summary>
/// Data transfer object for creating new vehicles
/// </summary>
/// <param name="Make">Vehicle manufacturer (e.g., Toyota, Honda)</param>
/// <param name="Model">Vehicle model (e.g., Camry, Civic)</param>
/// <param name="Year">Manufacturing year</param>
/// <param name="Price">Vehicle price in local currency</param>
/// <param name="Color">Vehicle color (optional)</param>
/// <param name="Description">Additional vehicle description (optional)</param>
public record VehicleCreateDto(string Make, string Model, int Year, decimal Price, string? Color, string? Description);

/// <summary>
/// Data transfer object for updating existing vehicles
/// Requires OTP code for security validation
/// </summary>
/// <param name="Make">Vehicle manufacturer</param>
/// <param name="Model">Vehicle model</param>
/// <param name="Year">Manufacturing year</param>
/// <param name="Price">Vehicle price in local currency</param>
/// <param name="Color">Vehicle color (optional)</param>
/// <param name="Description">Additional vehicle description (optional)</param>
/// <param name="OtpCode">One-time password for authorization</param>
public record VehicleUpdateDto(string Make, string Model, int Year, decimal Price, string? Color, string? Description, string OtpCode);

/// <summary>
/// Data transfer object for vehicle search and filtering
/// All parameters are optional for flexible querying
/// </summary>
/// <param name="Make">Filter by manufacturer</param>
/// <param name="Model">Filter by model</param>
/// <param name="MinYear">Minimum manufacturing year</param>
/// <param name="MaxYear">Maximum manufacturing year</param>
/// <param name="MinPrice">Minimum price range</param>
/// <param name="MaxPrice">Maximum price range</param>
public record VehicleQueryDto(string? Make, string? Model, int? MinYear, int? MaxYear, decimal? MinPrice, decimal? MaxPrice);
