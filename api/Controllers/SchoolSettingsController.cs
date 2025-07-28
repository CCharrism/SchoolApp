using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.DTOs;
using api.Models;
using System.Security.Claims;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Allow all authenticated users to access theme settings
    public class SchoolSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public SchoolSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<SchoolSettingsResponse>> GetSettings([FromQuery] int? schoolId = null)
        {
            int targetSchoolId;
            
            // Check user role and determine school ID
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole == "Admin")
            {
                // Admin must specify school ID in query parameter
                if (!schoolId.HasValue)
                {
                    return BadRequest("Admin users must specify schoolId parameter");
                }
                targetSchoolId = schoolId.Value;
            }
            else if (userRole == "Teacher")
            {
                // Teachers get school ID from their teacher record
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }
                
                var teacher = await _context.Teachers.FindAsync(teacherId);
                if (teacher == null)
                {
                    return Unauthorized("Teacher not found");
                }
                targetSchoolId = teacher.SchoolId;
            }
            else if (userRole == "Student")
            {
                // Students get school ID from their student record
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (!int.TryParse(userIdClaim, out int studentId))
                {
                    return Unauthorized("Invalid student credentials");
                }
                
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                {
                    return Unauthorized("Student not found");
                }
                targetSchoolId = student.SchoolId;
            }
            else
            {
                // SchoolOwner and SchoolHead get school ID from JWT token
                var schoolIdClaim = User.FindFirst("school_id")?.Value;
                if (!int.TryParse(schoolIdClaim, out targetSchoolId))
                {
                    return Unauthorized("Invalid school credentials");
                }
            }
            
            var settings = await _context.SchoolSettings
                .FirstOrDefaultAsync(s => s.SchoolId == targetSchoolId);
            
            if (settings == null)
            {
                // Create default settings if none exist
                var school = await _context.Schools.FindAsync(targetSchoolId);
                if (school == null)
                {
                    return NotFound("School not found");
                }
                
                settings = new SchoolSettings
                {
                    SchoolId = targetSchoolId,
                    SchoolDisplayName = school.SchoolName,
                    LogoImageUrl = "",
                    NavigationType = "sidebar",
                    ThemeColor = "#92DE8B"
                };
                
                _context.SchoolSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            
            var response = new SchoolSettingsResponse
            {
                Id = settings.Id,
                SchoolId = settings.SchoolId,
                SchoolDisplayName = settings.SchoolDisplayName,
                LogoImageUrl = settings.LogoImageUrl,
                NavigationType = settings.NavigationType,
                ThemeColor = settings.ThemeColor,
                UpdatedAt = settings.UpdatedAt
            };
            
            return Ok(response);
        }
        
        [HttpPut]
        [Authorize(Roles = "Admin,SchoolOwner,SchoolHead")] // Only these roles can modify settings
        public async Task<ActionResult<SchoolSettingsResponse>> UpdateSettings([FromBody] SchoolSettingsRequest request, [FromQuery] int? schoolId = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { 
                        Field = x.Key, 
                        Errors = x.Value?.Errors.Select(e => e.ErrorMessage) 
                    })
                    .ToList();
                
                return BadRequest(new { 
                    message = "Validation failed", 
                    errors = errors 
                });
            }
            
            int targetSchoolId;
            
            // Check user role and determine school ID
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (userRole == "Admin")
            {
                // Admin must specify school ID in query parameter
                if (!schoolId.HasValue)
                {
                    return BadRequest("Admin users must specify schoolId parameter");
                }
                targetSchoolId = schoolId.Value;
            }
            else
            {
                // SchoolOwner and SchoolHead get school ID from JWT token
                var schoolIdClaim = User.FindFirst("school_id")?.Value;
                if (!int.TryParse(schoolIdClaim, out targetSchoolId))
                {
                    return Unauthorized("Invalid school credentials");
                }
            }
            
            var settings = await _context.SchoolSettings
                .FirstOrDefaultAsync(s => s.SchoolId == targetSchoolId);
            
            if (settings == null)
            {
                // Create new settings
                settings = new SchoolSettings
                {
                    SchoolId = targetSchoolId
                };
                _context.SchoolSettings.Add(settings);
            }
            
            // Update settings
            settings.SchoolDisplayName = request.SchoolDisplayName;
            settings.LogoImageUrl = request.LogoImageUrl;
            settings.NavigationType = request.NavigationType;
            settings.ThemeColor = request.ThemeColor;
            settings.UpdatedAt = DateTime.UtcNow;
            
            // Also update the school name in the Schools table
            var school = await _context.Schools.FindAsync(targetSchoolId);
            if (school != null && !string.IsNullOrEmpty(request.SchoolDisplayName))
            {
                school.SchoolName = request.SchoolDisplayName;
            }
            
            await _context.SaveChangesAsync();
            
            var response = new SchoolSettingsResponse
            {
                Id = settings.Id,
                SchoolId = settings.SchoolId,
                SchoolDisplayName = settings.SchoolDisplayName,
                LogoImageUrl = settings.LogoImageUrl,
                NavigationType = settings.NavigationType,
                ThemeColor = settings.ThemeColor,
                UpdatedAt = settings.UpdatedAt
            };
            
            return Ok(response);
        }
    }
}
