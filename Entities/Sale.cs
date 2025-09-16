namespace CarDealership.Api.Entities;

public class Sale
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;
    public int CustomerId { get; set; }
    public User Customer { get; set; } = default!;
    public DateTime SoldAt { get; set; } = DateTime.UtcNow;
    public decimal Price { get; set; }
}
