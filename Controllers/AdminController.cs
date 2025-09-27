using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarDealership.Api.Controllers;

// This controller holds all admin-only features.
// It can only be used if the logged-in user has role = Admin.
// Routes start with /api/admin
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IUserService userService, IPurchaseService purchaseService) : ControllerBase
{
    /// <summary>
    /// Lists all registered customers (non-admin users)
    /// </summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers()
    {
        var customerList = await userService.GetAllCustomersAsync();
        return Ok(customerList);
    }

    /// <summary>
    /// Approves a pending purchase request and completes the sale
    /// Marks vehicle as unavailable and creates sale record
    /// </summary>
    [HttpPost("process-sale/{purchaseRequestId:int}")]
    public async Task<IActionResult> ProcessSale(int purchaseRequestId)
    {
        try
        {
            var saleResult = await purchaseService.ProcessPurchaseRequestAsync(purchaseRequestId);
            return Ok(saleResult);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
