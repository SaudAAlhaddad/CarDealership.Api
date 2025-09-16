namespace CarDealership.Api.Entities;

public enum OtpPurpose { Register = 1, Login = 2, Purchase = 3, UpdateVehicle = 4 }

public class OtpToken
{
    public int Id { get; set; }
    public string Subject { get; set; } = default!; 
    public OtpPurpose Purpose { get; set; }
    public string Code { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool Consumed { get; set; } = false;
}
