using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var userName = User.Identity?.Name;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            return Ok(new 
            { 
                message = "This is a protected endpoint",
                user = userName,
                role = userRole,
                timestamp = DateTime.UtcNow
            });
        }
        
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new 
            { 
                message = "This endpoint is only accessible by admins",
                user = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
