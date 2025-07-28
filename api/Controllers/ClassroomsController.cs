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
    public class ClassroomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClassroomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/classrooms
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ClassroomResponse>>> GetClassrooms()
        {
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            IQueryable<Classroom> query = _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.ClassroomStudents)
                .Include(c => c.Assignments)
                .Where(c => c.IsActive);

            // Filter based on role
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher != null)
                {
                    query = query.Where(c => c.TeacherId == teacher.Id);
                }
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                if (student != null)
                {
                    query = query.Where(c => c.ClassroomStudents.Any(cs => cs.StudentId == student.Id && cs.IsActive));
                }
            }

            var classrooms = await query.ToListAsync();

            var response = classrooms.Select(c => new ClassroomResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Subject = c.Subject,
                Section = c.Section,
                ClassCode = c.ClassCode,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher?.FullName ?? "Unknown",
                StudentCount = c.ClassroomStudents.Count(cs => cs.IsActive),
                AssignmentCount = c.Assignments.Count(a => a.IsActive),
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive
            }).ToList();

            return response;
        }

        // GET: api/classrooms/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ClassroomResponse>> GetClassroom(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.ClassroomStudents)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var response = new ClassroomResponse
            {
                Id = classroom.Id,
                Name = classroom.Name,
                Description = classroom.Description,
                Subject = classroom.Subject,
                Section = classroom.Section,
                ClassCode = classroom.ClassCode,
                TeacherId = classroom.TeacherId,
                TeacherName = classroom.Teacher?.FullName ?? "Unknown",
                StudentCount = classroom.ClassroomStudents.Count(cs => cs.IsActive),
                AssignmentCount = classroom.Assignments.Count(a => a.IsActive),
                CreatedAt = classroom.CreatedAt,
                IsActive = classroom.IsActive
            };

            return response;
        }

        // POST: api/classrooms - REMOVED
        // Only school heads can create classrooms via SchoolHeadController
        // Teachers can only view and manage their assigned classrooms

        // POST: api/classrooms/join
        [HttpPost("join")]
        [Authorize]
        public async Task<ActionResult> JoinClassroom(JoinClassroomRequest request)
        {
            Console.WriteLine($"ClassroomsController - JoinClassroom called with ClassCode: {request?.ClassCode}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"ClassroomsController - ModelState is invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
            
            Console.WriteLine($"ClassroomsController - userId: {userId}, role: {role}");
            Console.WriteLine($"ClassroomsController - All claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            

            // Only students can join classrooms
            if (role != "Student")
            {
                Console.WriteLine($"ClassroomsController - Access denied: role is {role}, not Student");
                return BadRequest(new { message = "Only students can join classrooms" });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            Console.WriteLine($"ClassroomsController - Student lookup for ID {userId}: {(student != null ? $"Found {student.FullName}" : "Not found")}");
            if (student == null)
            {
                return BadRequest("Student not found");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.ClassCode == request.ClassCode && c.IsActive);
            Console.WriteLine($"ClassroomsController - Classroom lookup for code {request?.ClassCode}: {(classroom != null ? $"Found {classroom.Name}" : "Not found")}");
            if (classroom == null)
            {
                return BadRequest("Invalid class code");
            }

            // Check if already enrolled
            var existingEnrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroom.Id && cs.StudentId == student.Id);

            Console.WriteLine($"ClassroomsController - Existing enrollment check: {(existingEnrollment != null ? $"Found enrollment, IsActive: {existingEnrollment.IsActive}" : "No existing enrollment")}");

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsActive)
                {
                    Console.WriteLine($"ClassroomsController - Student already actively enrolled");
                    return Ok(new { 
                        message = "You are already enrolled in this classroom",
                        classroomName = classroom.Name,
                        studentName = student.FullName,
                        alreadyEnrolled = true
                    });
                }
                else
                {
                    Console.WriteLine($"ClassroomsController - Reactivating inactive enrollment");
                    // Reactivate enrollment
                    existingEnrollment.IsActive = true;
                    existingEnrollment.JoinedAt = DateTime.UtcNow;
                }
            }
            else
            {
                Console.WriteLine($"ClassroomsController - Creating new enrollment");
                // Create new enrollment
                var enrollment = new ClassroomStudent
                {
                    ClassroomId = classroom.Id,
                    StudentId = student.Id,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.ClassroomStudents.Add(enrollment);
            }

            await _context.SaveChangesAsync();
            
            Console.WriteLine($"ClassroomsController - Successfully enrolled student {student.FullName} in classroom {classroom.Name}");
            
            return Ok(new { 
                message = "Successfully joined classroom",
                classroomName = classroom.Name,
                studentName = student.FullName
            });
        }

        // POST: api/classrooms/{id}/announcements
        [HttpPost("{id}/announcements")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<AnnouncementResponse>> CreateAnnouncement(int id, CreateAnnouncementRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
            
            if (teacher == null)
            {
                return BadRequest("Teacher not found");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            if (classroom == null)
            {
                return NotFound("Classroom not found");
            }

            // Check if teacher is assigned to this classroom
            if (classroom.TeacherId != teacher.Id)
            {
                return Forbid();
            }

            var announcement = new Announcement
            {
                Title = request.Title,
                Content = request.Content,
                ClassroomId = classroom.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var response = new AnnouncementResponse
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                ClassroomId = announcement.ClassroomId,
                ClassroomName = classroom.Name,
                AuthorName = teacher.FullName,
                CreatedAt = announcement.CreatedAt
            };

            return Ok(response);
        }

        // GET: api/classrooms/{id}/announcements
        [HttpGet("{id}/announcements")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AnnouncementResponse>>> GetClassroomAnnouncements(int id)
        {
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            var classroom = await _context.Classrooms
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
                
            if (classroom == null)
            {
                return NotFound("Classroom not found");
            }

            // Check access permissions
            bool hasAccess = false;
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                hasAccess = teacher != null && classroom.TeacherId == teacher.Id;
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                hasAccess = student != null && await _context.ClassroomStudents
                    .AnyAsync(cs => cs.ClassroomId == id && cs.StudentId == student.Id && cs.IsActive);
            }

            if (!hasAccess)
            {
                return Forbid();
            }

            var announcements = await _context.Announcements
                .Where(a => a.ClassroomId == id && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = classroom.Name,
                    AuthorName = classroom.Teacher.FullName,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(announcements);
        }

        // DELETE: api/classrooms/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroom = await _context.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            // Only the teacher who created the classroom can delete it
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher == null || classroom.TeacherId != teacher.Id)
                {
                    return Forbid("You can only delete your own classrooms");
                }
            }
            else if (role != "SchoolHead" && role != "Admin")
            {
                return Forbid("Insufficient permissions");
            }

            classroom.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateClassCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
