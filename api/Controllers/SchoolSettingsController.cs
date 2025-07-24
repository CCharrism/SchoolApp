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
    [Authorize(Roles = "SchoolOwner,SchoolHead")]
    public class SchoolSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public SchoolSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult<SchoolSettingsResponse>> GetSettings()
        {
            // Get school ID from JWT token
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!int.TryParse(schoolIdClaim, out int schoolId))
            {
                return Unauthorized("Invalid school owner");
            }
            
            var settings = await _context.SchoolSettings
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);
            
            if (settings == null)
            {
                // Create default settings if none exist
                var school = await _context.Schools.FindAsync(schoolId);
                if (school == null)
                {
                    return NotFound("School not found");
                }
                
                settings = new SchoolSettings
                {
                    SchoolId = schoolId,
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
        public async Task<ActionResult<SchoolSettingsResponse>> UpdateSettings([FromBody] SchoolSettingsRequest request)
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Get school ID from JWT token
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (!int.TryParse(schoolIdClaim, out int schoolId))
            {
                return Unauthorized("Invalid school owner");
            }
            
            var settings = await _context.SchoolSettings
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);
            
            if (settings == null)
            {
                // Create new settings
                settings = new SchoolSettings
                {
                    SchoolId = schoolId
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
            var school = await _context.Schools.FindAsync(schoolId);
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
