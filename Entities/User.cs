namespace CarDealership.Api.Entities;

/// <summary>
/// Defines user roles in the car dealership system
/// </summary>
public enum UserRole 
{ 
    Admin = 1,    // Can manage vehicles and process sales
    Customer = 2  // Can browse vehicles and make purchase requests
}

/// <summary>
/// Represents a user in the car dealership system
/// Can be either an Admin (manages system) or Customer (makes purchases)
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// User's email address (used for authentication and identification)
    /// Must be unique across all users
    /// </summary>
    public string Email { get; set; } = default!;
    
    /// <summary>
    /// Hashed version of the user's password for secure storage
    /// Never store plain text passwords
    /// </summary>
    public string PasswordHash { get; set; } = default!;
    
    /// <summary>
    /// User's role determining system permissions (Admin or Customer)
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Customer;
    
    /// <summary>
    /// User's full name for display purposes (optional)
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// Collection of sales made by this customer (only relevant for Customer role)
    /// </summary>
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
