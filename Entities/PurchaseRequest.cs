namespace CarDealership.Api.Entities;

/// <summary>
/// Defines the possible statuses for purchase requests
/// </summary>
public enum PurchaseStatus 
{ 
    Pending = 0,   // Awaiting admin approval
    Approved = 1,  // Approved by admin, sale can proceed
    Rejected = 2   // Rejected by admin
}

/// <summary>
/// Represents a customer's request to purchase a specific vehicle
/// Requires admin approval before the sale can be completed
/// </summary>
public class PurchaseRequest
{
    /// <summary>
    /// Unique identifier for the purchase request
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the vehicle the customer wants to purchase
    /// </summary>
    public int VehicleId { get; set; }
    
    /// <summary>
    /// ID of the customer making the purchase request
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// Navigation property to the requested vehicle
    /// </summary>
    public Vehicle Vehicle { get; set; } = default!;
    
    /// <summary>
    /// Timestamp when the purchase request was submitted
    /// Automatically set to current UTC time
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to the customer who made the request
    /// </summary>
    public User Customer { get; set; } = default!;
    
    /// <summary>
    /// Current status of the purchase request (Pending/Approved/Rejected)
    /// Defaults to Pending when created
    /// </summary>
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
}
