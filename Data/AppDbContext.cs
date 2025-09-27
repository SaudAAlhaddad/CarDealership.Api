using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Data;

/// <summary>
/// Entity Framework DbContext that maps C# entities to database tables
/// Handles database schema configuration, relationships, and constraints
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Define database tables/collections
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<OtpToken> OtpTokens => Set<OtpToken>();

    /// <summary>
    /// Configures entity relationships, constraints, and data conversions
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique(); // Prevent duplicate email addresses
        modelBuilder.Entity<User>()
            .Property(user => user.Role)
            .HasConversion<int>(); // Store enum as integer

        // PurchaseRequest entity relationships
        modelBuilder.Entity<PurchaseRequest>()
            .HasOne(purchaseRequest => purchaseRequest.Vehicle)
            .WithMany()
            .HasForeignKey(purchaseRequest => purchaseRequest.VehicleId);
        modelBuilder.Entity<PurchaseRequest>()
            .HasOne(purchaseRequest => purchaseRequest.Customer)
            .WithMany()
            .HasForeignKey(purchaseRequest => purchaseRequest.CustomerId);

        // Sale entity relationships
        modelBuilder.Entity<Sale>()
            .HasOne(sale => sale.Vehicle)
            .WithMany()
            .HasForeignKey(sale => sale.VehicleId);
        modelBuilder.Entity<Sale>()
            .HasOne(sale => sale.Customer)
            .WithMany(customer => customer.Sales)
            .HasForeignKey(sale => sale.CustomerId);

        // OTP token configuration
        modelBuilder.Entity<OtpToken>()
            .Property(otpToken => otpToken.Purpose)
            .HasConversion<int>(); // Store enum as integer
    }
}
