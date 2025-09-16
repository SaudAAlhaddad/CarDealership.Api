using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CarDealership.Api.Persistence;

// This method runs when the app starts
// It makes sure the database exists, create admin and sample vehicles

public static class DbInitializer
{
    public static async Task InitAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            // admin: admin@dealer.com / Password123!
            var admin = new User
            {
                Email = "admin@dealer.com",
                PasswordHash = Hash("Password123!"),
                Role = UserRole.Admin,
                FullName = "Admin User"
            };
            db.Users.Add(admin);
        }

        if (!await db.Vehicles.AnyAsync())
        {
            var seed = new[]
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
            db.Vehicles.AddRange(seed);
        }

        await db.SaveChangesAsync();
    }

    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
