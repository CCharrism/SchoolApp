using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BCrypt.Net;
using OfficeOpenXml;

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

        // POST: api/schoolhead/students/import-excel
        [HttpPost("students/import-excel")]
        public async Task<ActionResult<BulkImportResponse>> ImportStudentsFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only Excel files (.xlsx) are supported");
            }

            var schoolIdClaim = User.FindFirst("school_id")?.Value;
            if (string.IsNullOrEmpty(schoolIdClaim) || !int.TryParse(schoolIdClaim, out int schoolId))
            {
                return BadRequest("Unable to determine school context");
            }

            var results = new BulkImportResponse
            {
                SuccessCount = 0,
                ErrorCount = 0,
                Errors = new List<string>()
            };

            try
            {
                using var stream = file.OpenReadStream();
                
                // Try to create ExcelPackage - if license issue occurs, handle it gracefully
                OfficeOpenXml.ExcelPackage package;
                try
                {
                    package = new OfficeOpenXml.ExcelPackage(stream);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("license"))
                {
                    // Try setting license and retry
                    try 
                    {
                        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    }
                    catch { }
                    
                    stream.Seek(0, SeekOrigin.Begin);
                    package = new OfficeOpenXml.ExcelPackage(stream);
                }
                
                using (package)
                {
                    if (package.Workbook.Worksheets.Count == 0)
                {
                    return BadRequest("Excel file contains no worksheets");
                }
                
                var worksheet = package.Workbook.Worksheets[0];
                
                if (worksheet.Dimension == null)
                {
                    return BadRequest("Excel worksheet is empty");
                }
                
                var rowCount = worksheet.Dimension.Rows;
                
                Console.WriteLine($"Processing Excel file with {rowCount} rows");
                
                if (rowCount < 2)
                {
                    return BadRequest("Excel file must contain at least a header row and one data row");
                }
                
                for (int row = 2; row <= rowCount; row++) // Skip header row
                {
                    try
                    {
                        Console.WriteLine($"Processing row {row}");
                        
                        var fullName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var username = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var email = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var password = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                        var phone = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        var grade = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        var rollNumber = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                        var parentName = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                        var parentPhone = worksheet.Cells[row, 9].Value?.ToString()?.Trim();

                        Console.WriteLine($"Row {row} data: Name={fullName}, Username={username}, Email={email}");

                        // Validate required fields
                        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) || 
                            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || 
                            string.IsNullOrEmpty(grade) || string.IsNullOrEmpty(rollNumber))
                        {
                            results.Errors.Add($"Row {row}: Missing required fields (Full Name, Username, Email, Password, Grade, Roll Number)");
                            results.ErrorCount++;
                            Console.WriteLine($"Row {row} validation failed: missing required fields");
                            continue;
                        }

                        // Check if username already exists
                        var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Username == username);
                        if (existingStudent != null)
                        {
                            results.Errors.Add($"Row {row}: Username '{username}' already exists");
                            results.ErrorCount++;
                            continue;
                        }

                        // Create new student
                        var student = new Student
                        {
                            FullName = fullName,
                            Username = username,
                            Email = email ?? "",
                            Phone = phone ?? "",
                            Grade = grade ?? "",
                            RollNumber = rollNumber ?? "",
                            ParentName = parentName ?? "",
                            ParentPhone = parentPhone ?? "",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                            SchoolId = schoolId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Students.Add(student);
                        results.SuccessCount++;
                        Console.WriteLine($"Row {row} processed successfully: {fullName} ({username})");
                    }
                    catch (Exception ex)
                    {
                        results.Errors.Add($"Row {row}: {ex.Message}");
                        results.ErrorCount++;
                        Console.WriteLine($"Row {row} error: {ex.Message}");
                    }
                }

                if (results.SuccessCount > 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Successfully saved {results.SuccessCount} students to database");
                }

                return Ok(results);
            } // End of using block
        }
        catch (Exception ex)
            {
                Console.WriteLine($"Excel import error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Error processing Excel file: {ex.Message}");
            }
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
