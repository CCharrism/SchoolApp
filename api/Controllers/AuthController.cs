using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.DTOs;
using api.Services;
using BCrypt.Net;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        
        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // First, try to find admin user only
            Console.WriteLine($"AuthController - Looking for user: {request.Username}");
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "Admin");
                
            Console.WriteLine($"AuthController - Admin lookup result: {(adminUser != null ? $"Found admin user {adminUser.Username}" : "No admin user found")}");
                
            if (adminUser != null && BCrypt.Net.BCrypt.Verify(request.Password, adminUser.PasswordHash))
            {
                Console.WriteLine($"AuthController - Password verified for admin {adminUser.Username}");
                var adminToken = _jwtService.GenerateToken(adminUser.Id, adminUser.Username, adminUser.Role);
                var adminExpiry = _jwtService.GetTokenExpiry();
                
                return Ok(new LoginResponse
                {
                    Token = adminToken,
                    Username = adminUser.Username,
                    Role = adminUser.Role,
                    ExpiresAt = adminExpiry
                });
            }
            
            // Then, try to find school owner
            Console.WriteLine($"AuthController - Looking for school owner: {request.Username}");
            var schoolOwnerUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "SchoolOwner");
                
            Console.WriteLine($"AuthController - School owner lookup result: {(schoolOwnerUser != null ? $"Found school owner {schoolOwnerUser.Username}" : "No school owner found")}");
                
            if (schoolOwnerUser != null && BCrypt.Net.BCrypt.Verify(request.Password, schoolOwnerUser.PasswordHash))
            {
                Console.WriteLine($"AuthController - Password verified for school owner {schoolOwnerUser.Username}");
                
                // Find the school associated with this owner
                var school = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == schoolOwnerUser.Username && s.IsActive);
                    
                Console.WriteLine($"AuthController - School lookup result: {(school != null ? $"Found school {school.SchoolName} (ID: {school.Id})" : "No school found")}");
                    
                if (school != null)
                {
                    Console.WriteLine($"AuthController - Generating token for school owner with school_id: {school.Id}");
                    var ownerToken = _jwtService.GenerateToken(schoolOwnerUser.Id, schoolOwnerUser.Username, "SchoolOwner", school.SchoolName, null, school.Id);
                    var ownerExpiry = _jwtService.GetTokenExpiry();
                    
                    return Ok(new LoginResponse
                    {
                        Token = ownerToken,
                        Username = schoolOwnerUser.Username,
                        Role = "SchoolOwner",
                        ExpiresAt = ownerExpiry,
                        SchoolName = school.SchoolName
                    });
                }
            }
            
            // Finally, try to find school head
            Console.WriteLine($"AuthController - Looking for school head with username: {request.Username}");
            var schoolHead = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "SchoolHead");
                
            Console.WriteLine($"AuthController - School head lookup result: {(schoolHead != null ? $"Found user {schoolHead.Username} with role {schoolHead.Role}" : "No school head found")}");
                
            if (schoolHead != null && BCrypt.Net.BCrypt.Verify(request.Password, schoolHead.PasswordHash))
            {
                Console.WriteLine($"AuthController - School head found: {schoolHead.Username} (ID: {schoolHead.Id})");
                
                var branch = await _context.Branches
                    .Include(b => b.School)
                    .FirstOrDefaultAsync(b => b.SchoolHeadUsername == schoolHead.Username);
                    
                Console.WriteLine($"AuthController - Branch lookup result: {(branch != null ? $"Found branch {branch.BranchName} (ID: {branch.Id})" : "No branch found")}");
                    
                if (branch != null)
                {
                    Console.WriteLine($"AuthController - About to generate token with:");
                    Console.WriteLine($"  userId: {schoolHead.Id}");
                    Console.WriteLine($"  username: {schoolHead.Username}");
                    Console.WriteLine($"  role: SchoolHead");
                    Console.WriteLine($"  schoolName: {branch.School.SchoolName}");
                    Console.WriteLine($"  branchId: {branch.Id}");
                    Console.WriteLine($"  schoolId: {branch.School.Id}");
                    
                    var headToken = _jwtService.GenerateToken(schoolHead.Id, schoolHead.Username, "SchoolHead", branch.School.SchoolName, branch.Id, branch.School.Id);
                    var headExpiry = _jwtService.GetTokenExpiry();
                    
                    return Ok(new LoginResponse
                    {
                        Token = headToken,
                        Username = schoolHead.Username,
                        Role = "SchoolHead",
                        ExpiresAt = headExpiry,
                        SchoolName = branch.School.SchoolName,
                        BranchName = branch.BranchName
                    });
                }
            }
            
            return Unauthorized("Invalid username or password");
        }
        
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            return Ok(new { message = "Token is valid", user = User.Identity?.Name });
        }
    }
}
