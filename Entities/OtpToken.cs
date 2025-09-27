namespace CarDealership.Api.Entities;

/// <summary>
/// Defines the different purposes for which OTPs can be generated
/// </summary>
public enum OtpPurpose 
{ 
    Register = 1,      // User registration validation
    Login = 2,         // Login authentication
    Purchase = 3,      // Purchase authorization (vehicle-bound)
    UpdateVehicle = 4  // Vehicle update authorization
}

/// <summary>
/// Represents a one-time password token for secure operations
/// Used for registration, login, purchases, and sensitive admin operations
/// Purchase OTPs are bound to specific vehicles for enhanced security
/// </summary>
public class OtpToken
{
    /// <summary>
    /// Unique identifier for the OTP token
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Email address of the user for whom the OTP was generated
    /// </summary>
    public string Subject { get; set; } = default!; 
    
    /// <summary>
    /// Purpose for which this OTP was generated
    /// </summary>
    public OtpPurpose Purpose { get; set; }
    
    /// <summary>
    /// The actual 6-digit OTP code
    /// </summary>
    public string Code { get; set; } = default!;
    
    /// <summary>
    /// When this OTP expires (typically 2-5 minutes after generation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether this OTP has been used (prevents reuse)
    /// </summary>
    public bool Consumed { get; set; } = false;
    
    /// <summary>
    /// For purchase OTPs, binds the OTP to a specific vehicle
    /// This prevents using a purchase OTP for a different vehicle
    /// Null for non-purchase OTPs
    /// </summary>
    public int? VehicleId { get; set; }
}
