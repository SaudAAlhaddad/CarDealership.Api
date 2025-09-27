using CarDealership.Api.Dtos;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarDealership.Api.Controllers;

// This controller handles customer purchase actions
// Only customers can call these routes
[ApiController]
[Route("api/purchases")]
[Authorize(Roles = "Customer")]
public class PurchasesController(IPurchaseService purchaseService) : ControllerBase
{
    /// <summary>
    /// Extracts the authenticated customer's user ID from JWT token claims
    /// </summary>
    private int GetCustomerUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpPost("request")]
    public async Task<IActionResult> RequestPurchase([FromBody] PurchaseRequestDto purchaseRequest)
    {
        try
        {
            var customerId = GetCustomerUserId();
            var purchaseResult = await purchaseService.CreatePurchaseRequestAsync(
                purchaseRequest.VehicleId, 
                customerId, 
                purchaseRequest.OtpCode);
            return Ok(purchaseResult);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Returns a list of past purchases/sales for the authenticated customer
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var customerId = GetCustomerUserId();
        var purchaseHistory = await purchaseService.GetCustomerPurchaseHistoryAsync(customerId);
        return Ok(purchaseHistory);
    }
}
