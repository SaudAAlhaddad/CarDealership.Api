using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Api.Persistence;

/// <summary>
/// Database initialization utility that runs on application startup
/// Performs migrations and seeds the database with initial admin user and sample vehicles
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by running migrations and seeding initial data
    /// </summary>
    /// <param name="dbContext">The database context to initialize</param>
    public static async Task InitAsync(AppDbContext dbContext)
    {
        // Apply any pending database migrations
        await dbContext.Database.MigrateAsync();

        // Seed admin user if no users exist
        if (!await dbContext.Users.AnyAsync())
        {
            // Default admin credentials: admin@dealer.com / Password123!
            var defaultAdmin = new User
            {
                Email = "admin@dealer.com",
                PasswordHash = Hash("Password123!"),
                Role = UserRole.Admin,
                FullName = "Admin User"
            };
            dbContext.Users.Add(defaultAdmin);
        }

        // Seed sample vehicles if no vehicles exist
        if (!await dbContext.Vehicles.AnyAsync())
        {
            var sampleVehicles = new[]
            {
                new Vehicle { Make="Toyota", Model="Camry", Year=2021, Price=88000, Color="White" },
                new Vehicle { Make="Toyota", Model="Corolla", Year=2022, Price=76000, Color="Silver" },
                new Vehicle { Make="Honda", Model="Civic", Year=2020, Price=72000, Color="Black" },
                new Vehicle { Make="Honda", Model="Accord", Year=2023, Price=99000, Color="Blue" },
                new Vehicle { Make="Hyundai", Model="Sonata", Year=2021, Price=83000 },
                new Vehicle { Make="Kia", Model="K5", Year=2022, Price=85000 },
                new Vehicle { Make="Ford", Model="Mustang", Year=2019, Price=135000, Color="Red" },
                new Vehicle { Make="Chevrolet", Model="Malibu", Year=2020, Price=78000 },
                new Vehicle { Make="BMW", Model="330i", Year=2023, Price=210000 },
                new Vehicle { Make="Mercedes", Model="C200", Year=2022, Price=230000 }
            };
            dbContext.Vehicles.AddRange(sampleVehicles);
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a SHA256 hash of the input string (used for admin password)
    /// </summary>
    /// <param name="input">String to hash</param>
    /// <returns>Hexadecimal hash representation</returns>
    public static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes);
    }
}
