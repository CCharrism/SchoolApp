using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace api.Controllers
{
    [Route("api/schoolhead")]
    [ApiController]
    [Authorize(Roles = "SchoolHead")]
    public class SchoolHeadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchoolHeadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/schoolhead/classrooms
        [HttpPost("classrooms")]
        public async Task<ActionResult<ClassroomResponse>> CreateClassroom(CreateSchoolHeadClassroomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schoolHeadId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var schoolHead = await _context.Users
                .Where(u => u.Id == schoolHeadId && u.Role == "SchoolHead")
                .FirstOrDefaultAsync();

            if (schoolHead == null)
            {
                return BadRequest("School head not found");
            }

            // Get school ID from school head
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            // Verify teacher belongs to the same school
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == request.TeacherId && t.SchoolId == schoolId && t.IsActive);

            if (teacher == null)
            {
                return BadRequest("Teacher not found or doesn't belong to this school");
            }

            // Generate unique class code
            string classCode;
            do
            {
                classCode = GenerateClassCode();
            } while (await _context.Classrooms.AnyAsync(c => c.ClassCode == classCode));

            var classroom = new Classroom
            {
                Name = request.Name,
                Description = request.Description,
                Subject = request.Subject,
                Section = request.Section,
                ClassCode = classCode,
                TeacherId = teacher.Id,
                SchoolId = schoolId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            var response = new ClassroomResponse
            {
                Id = classroom.Id,
                Name = classroom.Name,
                Description = classroom.Description,
                Subject = classroom.Subject,
                Section = classroom.Section,
                ClassCode = classroom.ClassCode,
                TeacherId = classroom.TeacherId,
                TeacherName = teacher.FullName,
                StudentCount = 0,
                AssignmentCount = 0,
                CreatedAt = classroom.CreatedAt,
                IsActive = classroom.IsActive
            };

            return CreatedAtAction("GetClassroom", "Classrooms", new { id = classroom.Id }, response);
        }

        // GET: api/schoolhead/classrooms
        [HttpGet("classrooms")]
        public async Task<ActionResult<IEnumerable<ClassroomResponse>>> GetClassrooms()
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var classrooms = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.ClassroomStudents)
                .Include(c => c.Assignments)
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

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

        // PUT: api/schoolhead/classrooms/{id}/teacher
        [HttpPut("classrooms/{id}/teacher")]
        public async Task<ActionResult> AssignTeacher(int id, AssignTeacherRequest request)
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == schoolId);

            if (classroom == null)
            {
                return NotFound("Classroom not found");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == request.TeacherId && t.SchoolId == schoolId && t.IsActive);

            if (teacher == null)
            {
                return BadRequest("Teacher not found or doesn't belong to this school");
            }

            classroom.TeacherId = teacher.Id;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/schoolhead/classrooms/{classroomId}/students/{studentId}
        [HttpPost("classrooms/{classroomId}/students/{studentId}")]
        public async Task<ActionResult> AssignStudentToClassroom(int classroomId, int studentId)
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == classroomId && c.SchoolId == schoolId);

            if (classroom == null)
            {
                return NotFound("Classroom not found");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == schoolId && s.IsActive);

            if (student == null)
            {
                return BadRequest("Student not found or doesn't belong to this school");
            }

            // Check if already enrolled
            var existingEnrollment = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsActive)
                {
                    return BadRequest("Student is already enrolled in this classroom");
                }
                else
                {
                    existingEnrollment.IsActive = true;
                    existingEnrollment.JoinedAt = DateTime.UtcNow;
                }
            }
            else
            {
                var enrollment = new ClassroomStudent
                {
                    ClassroomId = classroomId,
                    StudentId = studentId,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.ClassroomStudents.Add(enrollment);
            }

            await _context.SaveChangesAsync();
            return Ok("Student assigned to classroom successfully");
        }

        // GET: api/schoolhead/teachers
        [HttpGet("teachers")]
        public async Task<ActionResult<IEnumerable<TeacherResponse>>> GetTeachers()
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive)
                .OrderBy(t => t.FullName)
                .ToListAsync();

            var response = teachers.Select(t => new TeacherResponse
            {
                Id = t.Id,
                FullName = t.FullName,
                Username = t.Username,
                Email = t.Email,
                Phone = t.Phone,
                Subject = t.Subject,
                Qualification = t.Qualification,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            }).ToList();

            return response;
        }

        // GET: api/schoolhead/students
        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents()
        {
            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var response = students.Select(s => new StudentResponse
            {
                Id = s.Id,
                FullName = s.FullName,
                Username = s.Username,
                Email = s.Email,
                Phone = s.Phone,
                Grade = s.Grade,
                RollNumber = s.RollNumber,
                ParentName = s.ParentName,
                ParentPhone = s.ParentPhone,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList();

            return response;
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
