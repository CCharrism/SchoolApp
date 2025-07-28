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
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students
                .Where(s => s.IsActive)
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        // GET: api/students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // POST: api/students
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Student>> PostStudent(CreateStudentRequest request)
        {
            // Check model validation
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Student creation validation failed:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            Console.WriteLine($"Creating student: {request.FullName} ({request.Username})");
            
            // Get school ID from the authenticated user's claims
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context. Please contact administrator.");
            }
            
            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            var student = new Student
            {
                FullName = request.FullName,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Phone = request.Phone,
                Grade = request.Grade,
                RollNumber = request.RollNumber,
                ParentName = request.ParentName,
                ParentPhone = request.ParentPhone,
                CreatedAt = DateTime.UtcNow,
                SchoolId = schoolId,
                IsActive = true
            };
            
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // PUT: api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // PUT: api/students/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStudentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
