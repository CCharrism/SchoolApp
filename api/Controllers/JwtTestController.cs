using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JwtTestController : ControllerBase
    {
        [HttpPost("decode")]
        public IActionResult DecodeToken([FromBody] TokenRequest request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(request.Token);
                
                var payload = new
                {
                    Header = token.Header,
                    Claims = token.Claims.Select(c => new { c.Type, c.Value }),
                    ValidFrom = token.ValidFrom,
                    ValidTo = token.ValidTo,
                    Issuer = token.Issuer,
                    Audiences = token.Audiences
                };
                
                Console.WriteLine($"🔍 JWT Decode - Token details: {JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })}");
                
                return Ok(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JWT Decode - Error: {ex.Message}");
                return BadRequest($"Invalid token: {ex.Message}");
            }
        }
        
        [HttpGet("test-auth")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult TestAuth()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            var roleClaim = User.FindFirst("role")?.Value;
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            
            Console.WriteLine($"🔍 JWT Test - User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"🔍 JWT Test - Claims: user_id={userIdClaim}, role={roleClaim}, school_id={schoolIdClaim}");
            
            return Ok(new { 
                message = "Authentication successful",
                userId = userIdClaim,
                role = roleClaim,
                schoolId = schoolIdClaim,
                isAuthenticated = User.Identity?.IsAuthenticated
            });
        }
        
        [HttpGet("headers")]
        public IActionResult GetHeaders()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            Console.WriteLine($"🔍 Headers - Request headers: {string.Join(", ", headers.Select(h => $"{h.Key}={h.Value}"))}");
            
            return Ok(new { 
                headers,
                hasAuthHeader = headers.ContainsKey("Authorization"),
                authHeader = headers.GetValueOrDefault("Authorization", "Not found")
            });
        }
    }
    
    public class TokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
