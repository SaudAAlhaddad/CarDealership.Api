namespace CarDealership.Api.Entities;

public class Sale
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public DateTime SoldAt { get; set; } = DateTime.UtcNow;
    public Vehicle Vehicle { get; set; } = default!;
    public User Customer { get; set; } = default!;
    public decimal Price { get; set; }
}
