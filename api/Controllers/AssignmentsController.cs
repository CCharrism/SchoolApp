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
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/assignments
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetAssignments()
        {
            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            IQueryable<Assignment> query = _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments)
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

            var assignments = await query.ToListAsync();

            var response = assignments.Select(a => new AssignmentResponse
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                Points = a.Points,
                ClassroomId = a.ClassroomId,
                ClassroomName = a.Classroom.Name,
                CreatedAt = a.CreatedAt,
                IsSubmitted = role == "Student" ? a.StudentAssignments.Any(sa => sa.StudentId == userId) : false,
                Grade = role == "Student" ? a.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.Grade : null,
                TeacherFeedback = role == "Student" ? a.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.TeacherFeedback : null
            }).ToList();

            return response;
        }

        // GET: api/assignments/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AssignmentResponse>> GetAssignment(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            var response = new AssignmentResponse
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                Points = assignment.Points,
                ClassroomId = assignment.ClassroomId,
                ClassroomName = assignment.Classroom.Name,
                CreatedAt = assignment.CreatedAt,
                IsSubmitted = role == "Student" ? assignment.StudentAssignments.Any(sa => sa.StudentId == userId) : false,
                Grade = role == "Student" ? assignment.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.Grade : null,
                TeacherFeedback = role == "Student" ? assignment.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.TeacherFeedback : null
            };

            return response;
        }

        // GET: api/assignments/classroom/5
        [HttpGet("classroom/{classroomId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetClassroomAssignments(int classroomId)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            
            Console.WriteLine($"AssignmentsController - GetClassroomAssignments request for classroom {classroomId}");
            Console.WriteLine($"AssignmentsController - user_id claim: {userIdClaim}");
            Console.WriteLine($"AssignmentsController - role claim: {roleClaim}");
            
            var userId = int.Parse(userIdClaim ?? "0");
            var role = roleClaim;

            // Verify user has access to this classroom
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId);
            if (classroom == null)
            {
                Console.WriteLine($"AssignmentsController - Classroom not found: {classroomId}");
                return NotFound("Classroom not found");
            }

            Console.WriteLine($"AssignmentsController - Found classroom: {classroom.Name} (ID: {classroom.Id})");

            bool hasAccess = false;
            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                hasAccess = teacher != null && classroom.TeacherId == teacher.Id;
                Console.WriteLine($"AssignmentsController - Teacher access check: hasAccess={hasAccess}, teacherId={teacher?.Id}, classroomTeacherId={classroom.TeacherId}");
            }
            else if (role == "Student")
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
                hasAccess = student != null && 
                    await _context.ClassroomStudents.AnyAsync(cs => 
                        cs.ClassroomId == classroomId && cs.StudentId == student.Id && cs.IsActive);
                Console.WriteLine($"AssignmentsController - Student access check: hasAccess={hasAccess}, studentId={student?.Id}");
            }

            if (!hasAccess)
            {
                Console.WriteLine($"AssignmentsController - Access denied for user {userId} with role {role} to classroom {classroomId}");
                return StatusCode(403, "You don't have access to this classroom");
            }

            var assignments = await _context.Assignments
                .Include(a => a.Classroom)
                .Include(a => a.StudentAssignments)
                .Where(a => a.ClassroomId == classroomId && a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"AssignmentsController - Found {assignments.Count} assignments for classroom {classroomId}");

            var response = assignments.Select(assignment => new AssignmentResponse
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                Points = assignment.Points,
                ClassroomId = assignment.ClassroomId,
                ClassroomName = assignment.Classroom.Name,
                CreatedAt = assignment.CreatedAt,
                IsSubmitted = role == "Student" ? assignment.StudentAssignments.Any(sa => sa.StudentId == userId) : false,
                Grade = role == "Student" ? assignment.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.Grade : null,
                TeacherFeedback = role == "Student" ? assignment.StudentAssignments.FirstOrDefault(sa => sa.StudentId == userId)?.TeacherFeedback : null
            }).ToList();

            Console.WriteLine($"AssignmentsController - Returning {response.Count} assignment responses");
            return Ok(response);
        }

        // POST: api/assignments
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AssignmentResponse>> PostAssignment(CreateAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role != "Teacher")
            {
                return Forbid("Only teachers can create assignments");
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
            if (teacher == null)
            {
                return BadRequest("Teacher not found");
            }

            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.TeacherId == teacher.Id);
            if (classroom == null)
            {
                return BadRequest("Classroom not found or you don't have permission to create assignments for this classroom");
            }

            var assignment = new Assignment
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                Points = request.Points,
                ClassroomId = request.ClassroomId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var response = new AssignmentResponse
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                Points = assignment.Points,
                ClassroomId = assignment.ClassroomId,
                ClassroomName = classroom.Name,
                CreatedAt = assignment.CreatedAt,
                IsSubmitted = false
            };

            return CreatedAtAction("GetAssignment", new { id = assignment.Id }, response);
        }

        // POST: api/assignments/5/submit
        [HttpPost("{id}/submit")]
        [Authorize]
        public async Task<ActionResult> SubmitAssignment(int id, SubmitAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role != "Student")
            {
                return Forbid("Only students can submit assignments");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == userId);
            if (student == null)
            {
                return BadRequest("Student not found");
            }

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .ThenInclude(c => c.ClassroomStudents)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound("Assignment not found");
            }

            // Check if student is enrolled in the classroom
            if (!assignment.Classroom.ClassroomStudents.Any(cs => cs.StudentId == student.Id && cs.IsActive))
            {
                return Forbid("You are not enrolled in this classroom");
            }

            // Check if already submitted
            var existingSubmission = await _context.StudentAssignments
                .FirstOrDefaultAsync(sa => sa.AssignmentId == id && sa.StudentId == student.Id);

            if (existingSubmission != null)
            {
                // Update existing submission
                existingSubmission.SubmissionText = request.SubmissionText;
                existingSubmission.SubmittedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new submission
                var submission = new StudentAssignment
                {
                    AssignmentId = id,
                    StudentId = student.Id,
                    SubmissionText = request.SubmissionText,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StudentAssignments.Add(submission);
            }

            await _context.SaveChangesAsync();
            return Ok("Assignment submitted successfully");
        }

        // PUT: api/assignments/5/grade
        [HttpPut("{id}/grade")]
        [Authorize]
        public async Task<ActionResult> GradeAssignment(int id, int studentId, GradeAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role != "Teacher")
            {
                return Forbid("Only teachers can grade assignments");
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
            if (teacher == null)
            {
                return BadRequest("Teacher not found");
            }

            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound("Assignment not found");
            }

            if (assignment.Classroom.TeacherId != teacher.Id)
            {
                return Forbid("You can only grade assignments from your own classrooms");
            }

            var submission = await _context.StudentAssignments
                .FirstOrDefaultAsync(sa => sa.AssignmentId == id && sa.StudentId == studentId);

            if (submission == null)
            {
                return NotFound("Student submission not found");
            }

            submission.Grade = request.Grade;
            submission.TeacherFeedback = request.TeacherFeedback;

            await _context.SaveChangesAsync();
            return Ok("Assignment graded successfully");
        }

        // DELETE: api/assignments/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("user_id")?.Value ?? "0");
            var role = User.FindFirst("role")?.Value;

            if (role == "Teacher")
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == userId);
                if (teacher == null || assignment.Classroom.TeacherId != teacher.Id)
                {
                    return Forbid("You can only delete your own assignments");
                }
            }
            else if (role != "SchoolHead" && role != "Admin")
            {
                return Forbid("Insufficient permissions");
            }

            assignment.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
