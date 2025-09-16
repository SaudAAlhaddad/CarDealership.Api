using CarDealership.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Data;

// This connects C# entity classes to the actual database tables

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<OtpToken> OtpTokens => Set<OtpToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Role).HasConversion<int>();
        b.Entity<PurchaseRequest>()
            .HasOne(pr => pr.Vehicle).WithMany().HasForeignKey(pr => pr.VehicleId);
        b.Entity<PurchaseRequest>()
            .HasOne(pr => pr.Customer).WithMany().HasForeignKey(pr => pr.CustomerId);
        b.Entity<Sale>()
            .HasOne(s => s.Vehicle).WithMany().HasForeignKey(s => s.VehicleId);
        b.Entity<Sale>()
            .HasOne(s => s.Customer).WithMany(u => u.Sales).HasForeignKey(s => s.CustomerId);
        b.Entity<OtpToken>().Property(o => o.Purpose).HasConversion<int>();
    }
}
