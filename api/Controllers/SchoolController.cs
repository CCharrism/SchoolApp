using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.DTOs;
using api.Models;
using BCrypt.Net;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchoolController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public SchoolController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SchoolResponse>> CreateSchool([FromBody] CreateSchoolRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Check if username already exists
            var existingUser = await _context.Users.AnyAsync(u => u.Username == request.OwnerUsername);
            var existingSchool = await _context.Schools.AnyAsync(s => s.OwnerUsername == request.OwnerUsername);
            
            if (existingUser || existingSchool)
            {
                return BadRequest("Username already exists");
            }
            
            // Get current admin user ID
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized("Invalid admin user");
            }
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Create school owner user
                var schoolOwner = new User
                {
                    Username = request.OwnerUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
                    Role = "SchoolOwner",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(schoolOwner);
                await _context.SaveChangesAsync();
                
                // Create school
                var school = new School
                {
                    SchoolName = request.SchoolName,
                    OwnerName = request.OwnerName,
                    OwnerUsername = request.OwnerUsername,
                    OwnerPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword),
                    Address = request.Address,
                    Phone = request.Phone,
                    Email = request.Email,
                    CreatedByAdminId = adminId,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Schools.Add(school);
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();
            
                // Load the created school with admin info
                var createdSchool = await _context.Schools
                    .Include(s => s.CreatedByAdmin)
                    .FirstAsync(s => s.Id == school.Id);
                
                var response = new SchoolResponse
                {
                    Id = createdSchool.Id,
                    SchoolName = createdSchool.SchoolName,
                    OwnerName = createdSchool.OwnerName,
                    OwnerUsername = createdSchool.OwnerUsername,
                    Address = createdSchool.Address,
                    Phone = createdSchool.Phone,
                    Email = createdSchool.Email,
                    IsActive = createdSchool.IsActive,
                    CreatedAt = createdSchool.CreatedAt,
                    CreatedByAdmin = createdSchool.CreatedByAdmin.Username
                };
                
                return CreatedAtAction(nameof(GetSchool), new { id = school.Id }, response);
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while creating the school");
            }
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<SchoolResponse>>> GetSchools()
        {
            var schools = await _context.Schools
                .Include(s => s.CreatedByAdmin)
                .OrderBy(s => s.SchoolName)
                .Select(s => new SchoolResponse
                {
                    Id = s.Id,
                    SchoolName = s.SchoolName,
                    OwnerName = s.OwnerName,
                    OwnerUsername = s.OwnerUsername,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    CreatedByAdmin = s.CreatedByAdmin.Username
                })
                .ToListAsync();
                
            return Ok(schools);
        }
        
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SchoolResponse>> GetSchool(int id)
        {
            var school = await _context.Schools
                .Include(s => s.CreatedByAdmin)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (school == null)
            {
                return NotFound();
            }
            
            var response = new SchoolResponse
            {
                Id = school.Id,
                SchoolName = school.SchoolName,
                OwnerName = school.OwnerName,
                OwnerUsername = school.OwnerUsername,
                Address = school.Address,
                Phone = school.Phone,
                Email = school.Email,
                IsActive = school.IsActive,
                CreatedAt = school.CreatedAt,
                CreatedByAdmin = school.CreatedByAdmin.Username
            };
            
            return Ok(response);
        }
        
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleSchoolStatus(int id)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null)
            {
                return NotFound();
            }
            
            school.IsActive = !school.IsActive;
            await _context.SaveChangesAsync();
            
            return Ok(new { message = $"School {(school.IsActive ? "activated" : "deactivated")} successfully" });
        }
    }
}
