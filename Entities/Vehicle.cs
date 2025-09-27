namespace CarDealership.Api.Entities;

/// <summary>
/// Represents a vehicle in the car dealership inventory
/// Contains all vehicle information and availability status
/// </summary>
public class Vehicle
{
    /// <summary>
    /// Unique identifier for the vehicle
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Vehicle manufacturer (e.g., Toyota, Honda, BMW)
    /// </summary>
    public string Make { get; set; } = default!;
    
    /// <summary>
    /// Vehicle model (e.g., Camry, Civic, 330i)
    /// </summary>
    public string Model { get; set; } = default!;
    
    /// <summary>
    /// Manufacturing year of the vehicle
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// Vehicle price in local currency
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Vehicle color (optional field)
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Indicates if the vehicle is available for purchase
    /// Set to false when a sale is processed
    /// </summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>
    /// Additional vehicle description or features (optional)
    /// </summary>
    public string? Description { get; set; }
}
