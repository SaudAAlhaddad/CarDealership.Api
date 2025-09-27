using CarDealership.Api.Dtos;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarDealership.Api.Controllers;

// This controller handles all vehicle actions
// Customers can browse cars and see details
// Admins can add or update vehicles 
[ApiController]
[Route("api/vehicles")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Browse([FromQuery] VehicleQueryDto queryFilters)
    {
        var availableVehicles = await vehicleService.BrowseVehiclesAsync(queryFilters);
        return Ok(availableVehicles);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int vehicleId)
    {
        var vehicleDetails = await vehicleService.GetVehicleByIdAsync(vehicleId);
        return vehicleDetails is null ? NotFound() : Ok(vehicleDetails);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] VehicleCreateDto newVehicleData)
    {
        var createdVehicle = await vehicleService.CreateVehicleAsync(newVehicleData);
        return CreatedAtAction(nameof(Details), new { id = createdVehicle.Id }, createdVehicle);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int vehicleId, [FromBody] VehicleUpdateDto updateData)
    {
        // Extract admin email from JWT token claims for OTP validation
        var adminEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(adminEmail)) return Unauthorized("Admin email not found in token.");

        try
        {
            var updatedVehicle = await vehicleService.UpdateVehicleAsync(vehicleId, updateData, adminEmail);
            return updatedVehicle is null ? NotFound("Vehicle not found.") : Ok(updatedVehicle);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
