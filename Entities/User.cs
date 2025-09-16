namespace CarDealership.Api.Entities;

public enum UserRole { Admin = 1, Customer = 2 }

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? FullName { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
