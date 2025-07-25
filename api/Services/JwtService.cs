using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;

namespace api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GenerateToken(User user)
        {
            return GenerateToken(user.Id, user.Username, user.Role);
        }
        
        public string GenerateToken(int userId, string username, string role, string? schoolName = null, int? branchId = null, int? schoolId = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            
            if (!string.IsNullOrEmpty(schoolName))
            {
                claims.Add(new Claim("school_name", schoolName));
            }
            
            // Add school_id claim for SchoolOwner role
            if (role == "SchoolOwner" && schoolId.HasValue)
            {
                claims.Add(new Claim("school_id", schoolId.Value.ToString()));
                Console.WriteLine($"Added school_id claim for SchoolOwner: {schoolId.Value}");
            }
            
            // Add branch_id and school_id claims for SchoolHead role
            if (role == "SchoolHead" && branchId.HasValue && schoolId.HasValue)
            {
                claims.Add(new Claim("branch_id", branchId.Value.ToString()));
                claims.Add(new Claim("school_id", schoolId.Value.ToString()));
                Console.WriteLine($"Added claims for SchoolHead - school_id: {schoolId.Value}, branch_id: {branchId.Value}");
            }
            else if (role == "SchoolHead")
            {
                Console.WriteLine($"SchoolHead role detected but branchId or schoolId is null or missing: branchId={branchId}, schoolId={schoolId}");
            }
            
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        public DateTime GetTokenExpiry()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            return DateTime.UtcNow.AddMinutes(expiryMinutes);
        }
    }
}
