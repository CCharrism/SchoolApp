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
    public class AnnouncementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/announcements
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AnnouncementResponse>>> GetAnnouncements()
        {
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            IQueryable<Announcement> query = _context.Announcements
                .Include(a => a.Classroom)
                .Where(a => a.IsActive);

            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher != null)
                {
                    query = query.Where(a => a.Classroom.TeacherId == teacher.Id);
                }
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                if (student != null)
                {
                    query = query.Where(a => a.Classroom.ClassroomStudents.Any(cs => cs.StudentId == student.Id && cs.IsActive));
                }
            }

            var announcements = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var response = announcements.Select(a => new AnnouncementResponse
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                ClassroomId = a.ClassroomId,
                ClassroomName = a.Classroom.Name,
                CreatedAt = a.CreatedAt
            }).ToList();

            return response;
        }

        // GET: api/announcements/classroom/{classroomId}
        [HttpGet("classroom/{classroomId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AnnouncementResponse>>> GetClassroomAnnouncements(int classroomId)
        {
            Console.WriteLine($"Getting announcements for classroom {classroomId}");
            
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;
            
            Console.WriteLine($"User ID: {userId}, Role: {role}");

            // Check if user has access to this classroom
            bool hasAccess = false;
            
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher != null)
                {
                    var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId && c.TeacherId == teacher.Id);
                    hasAccess = classroom != null;
                }
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                if (student != null)
                {
                    var enrollment = await _context.ClassroomStudents
                        .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == student.Id && cs.IsActive);
                    hasAccess = enrollment != null;
                }
            }
            else if (role == "SchoolHead" || role == "Admin")
            {
                hasAccess = true;
            }

            if (!hasAccess)
            {
                Console.WriteLine($"Access denied for user {userId} to classroom {classroomId}");
                return Forbid("You don't have access to this classroom");
            }

            var announcements = await _context.Announcements
                .Include(a => a.Classroom)
                .ThenInclude(c => c.Teacher)
                .Where(a => a.ClassroomId == classroomId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    AuthorName = a.Classroom.Teacher.FullName,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = a.Classroom.Name,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            Console.WriteLine($"Found {announcements.Count} announcements for classroom {classroomId}");
            return announcements;
        }

        // GET: api/announcements/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AnnouncementResponse>> GetAnnouncement(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            var response = new AnnouncementResponse
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Content = announcement.Content,
                ClassroomId = announcement.ClassroomId,
                ClassroomName = announcement.Classroom.Name,
                CreatedAt = announcement.CreatedAt
            };

            return response;
        }

        // POST: api/announcements
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AnnouncementResponse>> PostAnnouncement(CreateAnnouncementRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role != "Teacher")
            {
                return Forbid("Only teachers can create announcements");
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
            if (teacher == null)
            {
                return BadRequest("Teacher not found");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.TeacherId == teacher.Id);
            if (classroom == null)
            {
                return BadRequest("Classroom not found or you don't have permission to create announcements for this classroom");
            }

            var announcement = new Announcement
            {
                Title = request.Title,
                Content = request.Content,
                ClassroomId = request.ClassroomId,
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
                CreatedAt = announcement.CreatedAt
            };

            return CreatedAtAction("GetAnnouncement", new { id = announcement.Id }, response);
        }

        // DELETE: api/announcements/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher == null || announcement.Classroom.TeacherId != teacher.Id)
                {
                    return Forbid("You can only delete your own announcements");
                }
            }
            else if (role != "SchoolHead" && role != "Admin")
            {
                return Forbid("Insufficient permissions");
            }

            announcement.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
