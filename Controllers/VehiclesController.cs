using CarDealership.Api.Data;
using CarDealership.Api.Dtos;
using CarDealership.Api.Entities;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Controllers;

// This controller handles all vehicle actions
// Customers can browse cars and see details
// Admins can add or update vehicles 
[ApiController]
[Route("api/vehicles")]
public class VehiclesController(AppDbContext db, IOtpService otp) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Browse([FromQuery] VehicleQueryDto q)
    {
        var query = db.Vehicles.AsQueryable().Where(v => v.IsAvailable);

        if (!string.IsNullOrWhiteSpace(q.Make)) query = query.Where(v => v.Make == q.Make);
        if (!string.IsNullOrWhiteSpace(q.Model)) query = query.Where(v => v.Model == q.Model);
        if (q.MinYear.HasValue) query = query.Where(v => v.Year >= q.MinYear);
        if (q.MaxYear.HasValue) query = query.Where(v => v.Year <= q.MaxYear);
        if (q.MinPrice.HasValue) query = query.Where(v => v.Price >= q.MinPrice);
        if (q.MaxPrice.HasValue) query = query.Where(v => v.Price <= q.MaxPrice);

        var list = await query.OrderBy(v => v.Make).ThenBy(v => v.Model).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var v = await db.Vehicles.FindAsync(id);
        return v is null ? NotFound() : Ok(v);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] VehicleCreateDto dto)
    {
        var v = new Vehicle
        {
            Make = dto.Make, Model = dto.Model, Year = dto.Year,
            Price = dto.Price, Color = dto.Color, Description = dto.Description
        };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Details), new { id = v.Id }, v);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] VehicleUpdateDto dto, [FromServices] IHttpContextAccessor accessor)
    {
        var v = await db.Vehicles.FindAsync(id);
        if (v is null) return NotFound();

        var email = accessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type.Contains("email", StringComparison.OrdinalIgnoreCase))?.Value
                    ?? accessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var valid = await otp.ValidateAsync(email, OtpPurpose.UpdateVehicle, dto.OtpCode);
        if (!valid) return BadRequest("Invalid or expired OTP for update.");

        v.Make = dto.Make; v.Model = dto.Model; v.Year = dto.Year;
        v.Price = dto.Price; v.Color = dto.Color; v.Description = dto.Description;

        await db.SaveChangesAsync();
        return Ok(v);
    }
}
