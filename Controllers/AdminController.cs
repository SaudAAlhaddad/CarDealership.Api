using CarDealership.Api.Data;
using CarDealership.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await db.Users
            .Where(u => u.Role == UserRole.Customer)
            .Select(u => new { u.Id, u.Email, u.FullName })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpPost("process-sale/{purchaseRequestId:int}")]
    public async Task<IActionResult> ProcessSale(int purchaseRequestId)
    {
        var pr = await db.PurchaseRequests
            .Include(p => p.Vehicle)
            .SingleOrDefaultAsync(p => p.Id == purchaseRequestId);

        if (pr is null) return NotFound("Purchase request not found.");
        if (pr.Status != PurchaseStatus.Pending) return BadRequest("Request already decided.");

        var vehicle = pr.Vehicle;
        if (vehicle is null || !vehicle.IsAvailable) return BadRequest("Vehicle not available.");

        vehicle.IsAvailable = false;
        pr.Status = PurchaseStatus.Approved;

        var sale = new Entities.Sale
        {
            VehicleId = vehicle.Id,
            CustomerId = pr.CustomerId,
            Price = vehicle.Price
        };
        db.Sales.Add(sale);

        await db.SaveChangesAsync();
        return Ok(new { message = "Sale processed.", saleId = sale.Id });
    }
}
