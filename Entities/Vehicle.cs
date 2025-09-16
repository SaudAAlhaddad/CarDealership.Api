namespace CarDealership.Api.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? Description { get; set; }
}
