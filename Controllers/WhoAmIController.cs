using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/whoami")]
public class WhoAmIController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        return Ok(new {
            IsAuth = User.Identity?.IsAuthenticated,
            Name = User.Identity?.Name,
            Role = User.FindFirstValue(ClaimTypes.Role),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")
        });
    }
}
