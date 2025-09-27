using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Debug endpoint that returns information about the currently authenticated user
/// Useful for testing JWT token claims and authentication status
/// </summary>
[ApiController]
[Route("api/whoami")]
public class WhoAmIController : ControllerBase
{
    /// <summary>
    /// Returns current user information from JWT token claims
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var userInfo = new {
            IsAuth = User.Identity?.IsAuthenticated,
            Name = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        };
        
        return Ok(userInfo);
    }
}
