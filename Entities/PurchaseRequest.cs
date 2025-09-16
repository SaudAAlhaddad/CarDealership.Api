namespace CarDealership.Api.Entities;

public enum PurchaseStatus { Pending = 0, Approved = 1, Rejected = 2 }

public class PurchaseRequest
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;
    public int CustomerId { get; set; }
    public User Customer { get; set; } = default!;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
}
