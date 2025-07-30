using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.DTOs;
using api.Services;
using BCrypt.Net;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        
        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // First, try to find admin user only
            Console.WriteLine($"AuthController - Looking for user: {request.Username}");
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "Admin");
                
            Console.WriteLine($"AuthController - Admin lookup result: {(adminUser != null ? $"Found admin user {adminUser.Username}" : "No admin user found")}");
                
            if (adminUser != null && adminUser.IsActive && BCrypt.Net.BCrypt.Verify(request.Password, adminUser.PasswordHash))
            {
                Console.WriteLine($"AuthController - Password verified for admin {adminUser.Username}");
                var adminToken = _jwtService.GenerateToken(adminUser.Id, adminUser.Username, adminUser.Role);
                var adminExpiry = _jwtService.GetTokenExpiry();
                
                return Ok(new LoginResponse
                {
                    Token = adminToken,
                    Username = adminUser.Username,
                    Role = adminUser.Role,
                    ExpiresAt = adminExpiry,
                    User = new UserInfo
                    {
                        Id = adminUser.Id,
                        Username = adminUser.Username,
                        Role = adminUser.Role,
                        FullName = "Administrator"
                    }
                });
            }
            
            // Check if admin user exists but is inactive
            if (adminUser != null && !adminUser.IsActive)
            {
                return Unauthorized("Your account has been deactivated. Please contact an administrator.");
            }
            
            // Then, try to find school owner
            Console.WriteLine($"AuthController - Looking for school owner: {request.Username}");
            var schoolOwnerUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "SchoolOwner");
                
            Console.WriteLine($"AuthController - School owner lookup result: {(schoolOwnerUser != null ? $"Found school owner {schoolOwnerUser.Username}" : "No school owner found")}");
                
            if (schoolOwnerUser != null && schoolOwnerUser.IsActive && BCrypt.Net.BCrypt.Verify(request.Password, schoolOwnerUser.PasswordHash))
            {
                Console.WriteLine($"AuthController - Password verified for school owner {schoolOwnerUser.Username}");
                
                // Find the school associated with this owner
                var school = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == schoolOwnerUser.Username && s.IsActive);
                    
                Console.WriteLine($"AuthController - School lookup result: {(school != null ? $"Found school {school.SchoolName} (ID: {school.Id})" : "No school found")}");
                    
                if (school != null)
                {
                    Console.WriteLine($"AuthController - Generating token for school owner with school_id: {school.Id}");
                    var ownerToken = _jwtService.GenerateToken(schoolOwnerUser.Id, schoolOwnerUser.Username, "SchoolOwner", school.SchoolName, null, school.Id);
                    var ownerExpiry = _jwtService.GetTokenExpiry();
                    
                    return Ok(new LoginResponse
                    {
                        Token = ownerToken,
                        Username = schoolOwnerUser.Username,
                        Role = "SchoolOwner",
                        ExpiresAt = ownerExpiry,
                        SchoolName = school.SchoolName,
                        User = new UserInfo
                        {
                            Id = schoolOwnerUser.Id,
                            Username = schoolOwnerUser.Username,
                            Role = "SchoolOwner",
                            FullName = school.OwnerName,
                            SchoolName = school.SchoolName
                        }
                    });
                }
            }
            
            // Check if school owner exists but is inactive
            if (schoolOwnerUser != null && !schoolOwnerUser.IsActive)
            {
                return Unauthorized("Your account has been deactivated. Please contact an administrator.");
            }
            
            // Finally, try to find school head
            Console.WriteLine($"AuthController - Looking for school head with username: {request.Username}");
            var schoolHead = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Role == "SchoolHead");
                
            Console.WriteLine($"AuthController - School head lookup result: {(schoolHead != null ? $"Found user {schoolHead.Username} with role {schoolHead.Role}" : "No school head found")}");
                
            if (schoolHead != null && schoolHead.IsActive && BCrypt.Net.BCrypt.Verify(request.Password, schoolHead.PasswordHash))
            {
                Console.WriteLine($"AuthController - School head found: {schoolHead.Username} (ID: {schoolHead.Id})");
                
                var branch = await _context.Branches
                    .Include(b => b.School)
                    .FirstOrDefaultAsync(b => b.SchoolHeadUsername == schoolHead.Username);
                    
                Console.WriteLine($"AuthController - Branch lookup result: {(branch != null ? $"Found branch {branch.BranchName} (ID: {branch.Id})" : "No branch found")}");
                    
                if (branch != null)
                {
                    Console.WriteLine($"AuthController - About to generate token with:");
                    Console.WriteLine($"  userId: {schoolHead.Id}");
                    Console.WriteLine($"  username: {schoolHead.Username}");
                    Console.WriteLine($"  role: SchoolHead");
                    Console.WriteLine($"  schoolName: {branch.School.SchoolName}");
                    Console.WriteLine($"  branchId: {branch.Id}");
                    Console.WriteLine($"  schoolId: {branch.School.Id}");
                    
                    var headToken = _jwtService.GenerateToken(schoolHead.Id, schoolHead.Username, "SchoolHead", branch.School.SchoolName, branch.Id, branch.School.Id);
                    var headExpiry = _jwtService.GetTokenExpiry();
                    
                    return Ok(new LoginResponse
                    {
                        Token = headToken,
                        Username = schoolHead.Username,
                        Role = "SchoolHead",
                        ExpiresAt = headExpiry,
                        SchoolName = branch.School.SchoolName,
                        BranchName = branch.BranchName,
                        User = new UserInfo
                        {
                            Id = schoolHead.Id,
                            Username = schoolHead.Username,
                            Role = "SchoolHead",
                            FullName = "School Head",
                            SchoolName = branch.School.SchoolName
                        }
                    });
                }
            }
            
            // Check if school head exists but is inactive
            if (schoolHead != null && !schoolHead.IsActive)
            {
                return Unauthorized("Your account has been deactivated. Please contact an administrator.");
            }
            
            // Try to find teacher
            Console.WriteLine($"AuthController - Looking for teacher with username: {request.Username}");
            var teacher = await _context.Teachers
                .Include(t => t.School)
                .FirstOrDefaultAsync(t => t.Username == request.Username);
                
            Console.WriteLine($"AuthController - Teacher lookup result: {(teacher != null ? $"Found teacher {teacher.Username}" : "No teacher found")}");
            
            if (teacher != null && teacher.IsActive && !string.IsNullOrEmpty(teacher.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, teacher.PasswordHash))
            {
                Console.WriteLine($"AuthController - Teacher authentication successful for {teacher.Username}");
                
                var teacherToken = _jwtService.GenerateToken(teacher.Id, teacher.Username, "Teacher", teacher.School?.SchoolName ?? "Unknown School", null, teacher.SchoolId);
                var teacherExpiry = _jwtService.GetTokenExpiry();
                
                return Ok(new LoginResponse
                {
                    Token = teacherToken,
                    Username = teacher.Username,
                    Role = "Teacher",
                    ExpiresAt = teacherExpiry,
                    SchoolName = teacher.School?.SchoolName ?? "Unknown School",
                    User = new UserInfo
                    {
                        Id = teacher.Id,
                        Username = teacher.Username,
                        Role = "Teacher",
                        FullName = teacher.FullName,
                        SchoolName = teacher.School?.SchoolName ?? "Unknown School"
                    }
                });
            }
            
            // Try to find student
            Console.WriteLine($"AuthController - Looking for student with username: {request.Username}");
            var student = await _context.Students
                .Include(s => s.School)
                .FirstOrDefaultAsync(s => s.Username == request.Username);
                
            Console.WriteLine($"AuthController - Student lookup result: {(student != null ? $"Found student {student.Username}" : "No student found")}");
            
            if (student != null && student.IsActive && !string.IsNullOrEmpty(student.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash))
            {
                Console.WriteLine($"AuthController - Student authentication successful for {student.Username}");
                
                var studentToken = _jwtService.GenerateToken(student.Id, student.Username, "Student", student.School?.SchoolName ?? "Unknown School", null, student.SchoolId);
                var studentExpiry = _jwtService.GetTokenExpiry();
                
                return Ok(new LoginResponse
                {
                    Token = studentToken,
                    Username = student.Username,
                    Role = "Student",
                    ExpiresAt = studentExpiry,
                    SchoolName = student.School?.SchoolName ?? "Unknown School",
                    User = new UserInfo
                    {
                        Id = student.Id,
                        Username = student.Username,
                        Role = "Student",
                        FullName = student.FullName,
                        SchoolName = student.School?.SchoolName ?? "Unknown School"
                    }
                });
            }
            
            // Debug student authentication failure
            if (student != null)
            {
                Console.WriteLine($"AuthController - Student auth failure debug:");
                Console.WriteLine($"  IsActive: {student.IsActive}");
                Console.WriteLine($"  PasswordHash not null/empty: {!string.IsNullOrEmpty(student.PasswordHash)}");
                Console.WriteLine($"  Password verification: {BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash)}");
            }
            
            return Unauthorized("Invalid username or password");
        }
        
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            return Ok(new { message = "Token is valid", user = User.Identity?.Name });
        }

        [HttpPost("debug/create-bgss-user")]
        public async Task<IActionResult> CreateBgssUser()
        {
            try
            {
                // Check if bgss user already exists
                var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Username == "bgss");
                if (existingStudent != null)
                {
                    return Ok(new { message = "User bgss already exists" });
                }

                // Get the first school to assign the student to
                var school = await _context.Schools.FirstOrDefaultAsync();
                if (school == null)
                {
                    return BadRequest(new { error = "No school found to assign student" });
                }

                // Create new student
                var newStudent = new Models.Student
                {
                    Username = "bgss",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    FullName = "BGSS User",
                    Email = "bgss@test.com",
                    Grade = "10",
                    RollNumber = "BGSS001",
                    SchoolId = school.Id,
                    ParentName = "BGSS Parent",
                    ParentPhone = "1234567890",
                    Phone = "0987654321",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(newStudent);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User bgss created successfully", studentId = newStudent.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("debug/update-bgss-password")]
        public async Task<IActionResult> UpdateBgssPassword()
        {
            try
            {
                // Find the bgss user
                var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.Username == "bgss");
                if (existingStudent == null)
                {
                    return BadRequest(new { error = "User bgss not found" });
                }

                // Update the password hash
                existingStudent.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                await _context.SaveChangesAsync();

                return Ok(new { message = "Password updated successfully for user bgss" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
