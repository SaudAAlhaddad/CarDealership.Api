namespace CarDealership.Api.Dtos;

public record VehicleCreateDto(string Make, string Model, int Year, decimal Price, string? Color, string? Description);
public record VehicleUpdateDto(string Make, string Model, int Year, decimal Price, string? Color, string? Description, string OtpCode);
public record VehicleQueryDto(string? Make, string? Model, int? MinYear, int? MaxYear, decimal? MinPrice, decimal? MaxPrice);
