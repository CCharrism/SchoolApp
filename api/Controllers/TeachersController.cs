using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/teachers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
        {
            return await _context.Teachers
                .Where(t => t.IsActive)
                .OrderBy(t => t.FullName)
                .ToListAsync();
        }

        // GET: api/teachers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            return teacher;
        }

        // POST: api/teachers
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Teacher>> PostTeacher(CreateTeacherRequest request)
        {
            // Check model validation
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Teacher creation validation failed:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            Console.WriteLine($"Creating teacher: {request.FullName} ({request.Username})");
            
            // Get school ID from the authenticated user's claims
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context. Please contact administrator.");
            }
            
            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            var teacher = new Teacher
            {
                FullName = request.FullName,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Phone = request.Phone,
                Subject = request.Subject,
                Qualification = request.Qualification,
                CreatedAt = DateTime.UtcNow,
                SchoolId = schoolId,
                IsActive = true
            };
            
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeacher", new { id = teacher.Id }, teacher);
        }

        // PUT: api/teachers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeacher(int id, Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return BadRequest();
            }

            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/teachers/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTeacherStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/teachers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
