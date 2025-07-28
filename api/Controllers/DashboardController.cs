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
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/teacher
        [HttpGet("teacher")]
        [Authorize]
        public async Task<ActionResult<TeacherDashboardResponse>> GetTeacherDashboard()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            
            Console.WriteLine($"DashboardController - Teacher dashboard request");
            Console.WriteLine($"DashboardController - user_id claim: {userIdClaim}");
            Console.WriteLine($"DashboardController - role claim: {roleClaim}");
            
            var userId = int.Parse(userIdClaim ?? "0");
            var role = roleClaim;

            if (role != "Teacher")
            {
                Console.WriteLine($"DashboardController - Role mismatch. Expected Teacher, got: {role}");
                return BadRequest("Only teachers can access teacher dashboard");
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
            if (teacher == null)
            {
                Console.WriteLine($"DashboardController - Teacher not found with ID: {userId}");
                return BadRequest("Teacher not found");
            }
            
            Console.WriteLine($"DashboardController - Found teacher: {teacher.FullName} (ID: {teacher.Id})");

            // Get teacher's classrooms
            var classrooms = await _context.Classrooms
                .Include(c => c.ClassroomStudents)
                .Include(c => c.Assignments)
                .Where(c => c.TeacherId == teacher.Id && c.IsActive)
                .ToListAsync();

            // Get recent assignments
            var recentAssignments = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments)
                .Where(a => a.Classroom.TeacherId == teacher.Id && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get recent announcements
            var recentAnnouncements = await _context.Announcements
                .Include(a => a.Classroom)
                .Where(a => a.Classroom.TeacherId == teacher.Id && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var totalStudents = classrooms.Sum(c => c.ClassroomStudents.Count(cs => cs.IsActive));
            var pendingAssignments = recentAssignments.Sum(a => a.StudentAssignments.Count(sa => sa.Grade == null));

            var response = new TeacherDashboardResponse
            {
                TotalClassrooms = classrooms.Count,
                TotalStudents = totalStudents,
                PendingAssignments = pendingAssignments,
                RecentClassrooms = classrooms.Take(3).Select(c => new ClassroomResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Subject = c.Subject,
                    Section = c.Section,
                    ClassCode = c.ClassCode,
                    TeacherId = c.TeacherId,
                    TeacherName = teacher.FullName,
                    StudentCount = c.ClassroomStudents.Count(cs => cs.IsActive),
                    AssignmentCount = c.Assignments.Count(a => a.IsActive),
                    CreatedAt = c.CreatedAt,
                    IsActive = c.IsActive
                }).ToList(),
                RecentAssignments = recentAssignments.Select(a => new AssignmentResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    Points = a.Points,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = a.Classroom.Name,
                    CreatedAt = a.CreatedAt
                }).ToList(),
                RecentAnnouncements = recentAnnouncements.Select(a => new AnnouncementResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = a.Classroom.Name,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };

            return response;
        }

        // GET: api/dashboard/student
        [HttpGet("student")]
        [Authorize]
        public async Task<ActionResult<StudentDashboardResponse>> GetStudentDashboard()
        {
            Console.WriteLine("DashboardController - GetStudentDashboard called");
            
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine($"DashboardController - userId: {userId}, role: {role}");
            Console.WriteLine($"DashboardController - All claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

            if (role != "Student")
            {
                Console.WriteLine($"DashboardController - Access denied: role is {role}, not Student");
                return BadRequest("Only students can access student dashboard");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            Console.WriteLine($"DashboardController - Student lookup for ID {userId}: {(student != null ? $"Found {student.FullName}" : "Not found")}");
            if (student == null)
            {
                return BadRequest("Student not found");
            }

            // Get student's enrolled classrooms
            var enrolledClassrooms = await _context.ClassroomStudents
                .Include(cs => cs.Classroom)
                .ThenInclude(c => c.Teacher)
                .Include(cs => cs.Classroom)
                .ThenInclude(c => c.Assignments)
                .Where(cs => cs.StudentId == student.Id && cs.IsActive)
                .Select(cs => cs.Classroom)
                .ToListAsync();

            // Get student's assignments
            var studentAssignments = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments.Where(sa => sa.StudentId == student.Id))
                .Where(a => a.Classroom.ClassroomStudents.Any(cs => cs.StudentId == student.Id && cs.IsActive) && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get student's announcements
            var studentAnnouncements = await _context.Announcements
                .Include(a => a.Classroom)
                .Where(a => a.Classroom.ClassroomStudents.Any(cs => cs.StudentId == student.Id && cs.IsActive) && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var pendingAssignments = studentAssignments.Count(a => !a.StudentAssignments.Any());
            var upcomingTests = studentAssignments.Count(a => a.DueDate > DateTime.UtcNow && a.DueDate <= DateTime.UtcNow.AddDays(7));

            var response = new StudentDashboardResponse
            {
                EnrolledClasses = enrolledClassrooms.Count,
                PendingAssignments = pendingAssignments,
                UpcomingTests = upcomingTests,
                Classrooms = enrolledClassrooms.Select(c => new ClassroomResponse
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
                }).ToList(),
                EnrolledClassrooms = enrolledClassrooms.Select(c => new ClassroomResponse
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
                }).ToList(),
                RecentAssignments = studentAssignments.Select(a => new AssignmentResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DueDate = a.DueDate,
                    Points = a.Points,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = a.Classroom.Name,
                    CreatedAt = a.CreatedAt,
                    IsSubmitted = a.StudentAssignments.Any(),
                    Grade = a.StudentAssignments.FirstOrDefault()?.Grade,
                    TeacherFeedback = a.StudentAssignments.FirstOrDefault()?.TeacherFeedback
                }).ToList(),
                RecentAnnouncements = studentAnnouncements.Select(a => new AnnouncementResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    ClassroomId = a.ClassroomId,
                    ClassroomName = a.Classroom.Name,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };

            return response;
        }
    }
}
