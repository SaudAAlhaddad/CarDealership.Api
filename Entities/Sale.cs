namespace CarDealership.Api.Entities;

/// <summary>
/// Represents a completed vehicle sale transaction
/// Created when an admin processes an approved purchase request
/// </summary>
public class Sale
{
    /// <summary>
    /// Unique identifier for the sale record
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the vehicle that was sold
    /// </summary>
    public int VehicleId { get; set; }
    
    /// <summary>
    /// ID of the customer who purchased the vehicle
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Timestamp when the sale was completed
    /// Automatically set to current UTC time
    /// </summary>
    public DateTime SoldAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to the sold vehicle
    /// </summary>
    public Vehicle Vehicle { get; set; } = default!;
    
    /// <summary>
    /// Navigation property to the customer who made the purchase
    /// </summary>
    public User Customer { get; set; } = default!;
    
    /// <summary>
    /// Price at which the vehicle was sold
    /// Captured at time of sale for historical accuracy
    /// </summary>
    public decimal Price { get; set; }
}
