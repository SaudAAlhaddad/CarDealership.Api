using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarDealership.Api.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "Customer")]
public class PurchasesController(AppDbContext db, IOtpService otp) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    // Purchase Request MUST be OTP-protected (purpose = Purchase)
    [HttpPost("request")]
    public async Task<IActionResult> RequestPurchase([FromBody] PurchaseRequestDto dto)
    {
        var userId = GetUserId();
        var user = await db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        var ok = await otp.ValidateAsync(user.Email, OtpPurpose.Purchase, dto.OtpCode);
        if (!ok) return BadRequest("Invalid or expired OTP for purchase.");

        var v = await db.Vehicles.FindAsync(dto.VehicleId);
        if (v is null || !v.IsAvailable) return BadRequest("Vehicle not available.");

        var pr = new PurchaseRequest
        {
            VehicleId = v.Id,
            CustomerId = userId
        };
        db.PurchaseRequests.Add(pr);
        await db.SaveChangesAsync();
        return Ok(new { message = "Purchase request submitted.", requestId = pr.Id });
    }

    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var userId = GetUserId();
        var sales = await db.Sales
            .Include(s => s.Vehicle)
            .Where(s => s.CustomerId == userId)
            .Select(s => new
            {
                s.Id,
                s.SoldAt,
                s.Price,
                Vehicle = new { s.Vehicle.Make, s.Vehicle.Model, s.Vehicle.Year }
            })
            .ToListAsync();
        return Ok(sales);
    }
}
