using api.Data;
using api.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        
        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
            
            // Check if admin user already exists
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }

            // Seed missing School Owner Users for existing schools
            await SeedMissingSchoolOwnerUsers();

            // Check if demo school owner already exists
            var schoolOwnerExists = await _context.Schools.AnyAsync(s => s.OwnerUsername == "owner1");
            
            if (!schoolOwnerExists)
            {
                // Create demo school owner user
                var demoOwnerUser = new User
                {
                    Username = "owner1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                    Role = "SchoolOwner",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(demoOwnerUser);
                await _context.SaveChangesAsync();

                var demoSchool = new School
                {
                    SchoolName = "Greenwood Academy",
                    OwnerName = "John Smith",
                    OwnerUsername = "owner1",
                    OwnerPasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                    Address = "123 Education Street, Learning City",
                    Phone = "+1-555-0123",
                    Email = "admin@greenwoodacademy.edu",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByAdminId = adminUser.Id
                };
                
                _context.Schools.Add(demoSchool);
                await _context.SaveChangesAsync();

                // Create demo branches and school heads
                await SeedDemoBranchesAndHeads(demoSchool.Id);
            }

            // Seed additional demo schools for better hierarchy visualization
            await SeedAdditionalDemoSchools(adminUser.Id);

            // Seed teachers, students, and classrooms
            await SeedTeachersStudentsAndClassrooms();
        }

        private async Task SeedMissingSchoolOwnerUsers()
        {
            var schools = await _context.Schools
                .Where(s => s.IsActive)
                .ToListAsync();

            foreach (var school in schools)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == school.OwnerUsername && u.Role == "SchoolOwner");

                if (existingUser == null)
                {
                    var schoolOwnerUser = new User
                    {
                        Username = school.OwnerUsername,
                        PasswordHash = school.OwnerPasswordHash,
                        Role = "SchoolOwner",
                        CreatedAt = school.CreatedAt
                    };

                    _context.Users.Add(schoolOwnerUser);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedDemoBranchesAndHeads(int schoolId)
        {
            var branches = new[]
            {
                new { Name = "Main Campus", Location = "Downtown Branch", HeadUsername = "head1", HeadName = "Dr. Sarah Johnson" },
                new { Name = "North Branch", Location = "North District", HeadUsername = "head2", HeadName = "Prof. Michael Brown" },
                new { Name = "South Campus", Location = "South Side", HeadUsername = "head3", HeadName = "Dr. Emily Davis" }
            };

            foreach (var branchData in branches)
            {
                var existingBranch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.SchoolId == schoolId && b.BranchName == branchData.Name);

                if (existingBranch == null)
                {
                    // Create school head user
                    var headUser = new User
                    {
                        Username = branchData.HeadUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("head123"),
                        Role = "SchoolHead",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(headUser);
                    await _context.SaveChangesAsync();

                    // Create branch
                    var branch = new Branch
                    {
                        BranchName = branchData.Name,
                        Description = $"This is the {branchData.Name.ToLower()} of Greenwood Academy",
                        Location = branchData.Location,
                        SchoolId = schoolId,
                        SchoolHeadUsername = branchData.HeadUsername,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();

                    // Add some demo courses
                    var courses = new[]
                    {
                        "Mathematics Advanced",
                        "English Literature",
                        "Computer Science",
                        "Physics",
                        "Chemistry"
                    };

                    foreach (var courseName in courses.Take(3)) // Add 3 courses per branch
                    {
                        var course = new Course
                        {
                            CourseName = courseName,
                            Description = $"Comprehensive {courseName} course",
                            BranchId = branch.Id,
                            CreatedBy = branchData.HeadUsername,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Courses.Add(course);
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedAdditionalDemoSchools(int adminId)
        {
            var additionalSchools = new[]
            {
                new { Name = "Riverside High School", Owner = "Maria Garcia", Username = "owner2", Email = "admin@riversidehigh.edu", Phone = "+1-555-0234" },
                new { Name = "Oakwood Elementary", Owner = "David Wilson", Username = "owner3", Email = "admin@oakwoodelementary.edu", Phone = "+1-555-0345" }
            };

            foreach (var schoolData in additionalSchools)
            {
                var existingSchool = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == schoolData.Username);

                if (existingSchool == null)
                {
                    // Create school owner user
                    var ownerUser = new User
                    {
                        Username = schoolData.Username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                        Role = "SchoolOwner",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(ownerUser);
                    await _context.SaveChangesAsync();

                    // Create school
                    var school = new School
                    {
                        SchoolName = schoolData.Name,
                        OwnerName = schoolData.Owner,
                        OwnerUsername = schoolData.Username,
                        OwnerPasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                        Address = $"456 {schoolData.Name} Street, Education District",
                        Phone = schoolData.Phone,
                        Email = schoolData.Email,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByAdminId = adminId
                    };

                    _context.Schools.Add(school);
                    await _context.SaveChangesAsync();

                    // Add fewer branches for variety
                    if (schoolData.Username == "owner2")
                    {
                        await SeedBranchForSchool(school.Id, "Main Building", "Central Location", "head4", "Dr. Lisa Anderson");
                        await SeedBranchForSchool(school.Id, "Sports Complex", "Athletic Grounds", "head5", "Coach Robert Taylor");
                    }
                    else if (schoolData.Username == "owner3")
                    {
                        await SeedBranchForSchool(school.Id, "Elementary Wing", "Main Campus", "head6", "Ms. Jennifer White");
                    }
                }
            }
        }

        private async Task SeedBranchForSchool(int schoolId, string branchName, string location, string headUsername, string headName)
        {
            // Create school head user
            var headUser = new User
            {
                Username = headUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("head123"),
                Role = "SchoolHead",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(headUser);
            await _context.SaveChangesAsync();

            // Create branch
            var branch = new Branch
            {
                BranchName = branchName,
                Description = $"The {branchName.ToLower()} facility",
                Location = location,
                SchoolId = schoolId,
                SchoolHeadUsername = headUsername,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            // Add 2 demo courses
            var courses = new[] { "English", "Mathematics" };
            foreach (var courseName in courses)
            {
                var course = new Course
                {
                    CourseName = courseName,
                    Description = $"Basic {courseName} course",
                    BranchId = branch.Id,
                    CreatedBy = headUsername,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Courses.Add(course);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedTeachersStudentsAndClassrooms()
        {
            // Check if teachers already exist
            var existingTeachers = await _context.Teachers.AnyAsync();
            if (existingTeachers) return;

            // Get first school for demo data
            var school = await _context.Schools.FirstOrDefaultAsync();
            if (school == null) return;

            // Create sample teachers
            var teachers = new[]
            {
                new Teacher
                {
                    Username = "teacher",
                    Email = "teacher@school.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                    FullName = "Demo Teacher",
                    Subject = "Mathematics",
                    Qualification = "M.Sc. Mathematics",
                    Phone = "555-0100",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Teacher
                {
                    Username = "john.smith",
                    Email = "john.smith@school.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                    FullName = "John Smith",
                    Subject = "Mathematics",
                    Qualification = "M.Sc. Mathematics",
                    Phone = "555-0101",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Teacher
                {
                    Username = "sarah.johnson",
                    Email = "sarah.johnson@school.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                    FullName = "Sarah Johnson",
                    Subject = "English",
                    Qualification = "M.A. English Literature",
                    Phone = "555-0102",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Teacher
                {
                    Username = "michael.brown",
                    Email = "michael.brown@school.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                    FullName = "Michael Brown",
                    Subject = "Science",
                    Qualification = "Ph.D. Physics",
                    Phone = "555-0103",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Teachers.AddRange(teachers);
            await _context.SaveChangesAsync();

            // Create sample students
            var students = new[]
            {
                new Student
                {
                    Username = "student",
                    Email = "student@school.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    FullName = "Demo Student",
                    Grade = "Grade 10",
                    RollNumber = "2024000",
                    ParentName = "Demo Parent",
                    ParentPhone = "555-1000",
                    Phone = "555-2000",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Student
                {
                    Username = "alice.wilson",
                    Email = "alice.wilson@student.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    FullName = "Alice Wilson",
                    Grade = "Grade 10",
                    RollNumber = "2024001",
                    ParentName = "Robert Wilson",
                    ParentPhone = "555-1001",
                    Phone = "555-2001",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Student
                {
                    Username = "bob.davis",
                    Email = "bob.davis@student.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    FullName = "Bob Davis",
                    Grade = "Grade 10",
                    RollNumber = "2024002",
                    ParentName = "Jennifer Davis",
                    ParentPhone = "555-1002",
                    Phone = "555-2002",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Student
                {
                    Username = "carol.miller",
                    Email = "carol.miller@student.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                    FullName = "Carol Miller",
                    Grade = "Grade 9",
                    RollNumber = "2024003",
                    ParentName = "David Miller",
                    ParentPhone = "555-1003",
                    Phone = "555-2003",
                    IsActive = true,
                    SchoolId = school.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();

            // Create sample classrooms
            var classrooms = new[]
            {
                new Classroom
                {
                    Name = "Grade 10 Mathematics",
                    Description = "Mathematics classroom for Grade 10 students",
                    Section = "Grade 10",
                    Subject = "Mathematics",
                    ClassCode = "MATH10A",
                    TeacherId = teachers[0].Id, // John Smith
                    SchoolId = school.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Classroom
                {
                    Name = "Grade 9 Science",
                    Description = "Science classroom for Grade 9 students",
                    Section = "Grade 9",
                    Subject = "Science",
                    ClassCode = "SCI9A",
                    TeacherId = teachers[2].Id, // Michael Brown
                    SchoolId = school.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Classrooms.AddRange(classrooms);
            await _context.SaveChangesAsync();

            // Create sample classroom enrollments
            var enrollments = new[]
            {
                // Enroll Alice Wilson (Grade 10) in Grade 10 Math Class
                new ClassroomStudent
                {
                    ClassroomId = classrooms[0].Id,
                    StudentId = students[0].Id, // Alice Wilson
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                },
                // Enroll Bob Davis (Grade 10) in Grade 10 Math Class
                new ClassroomStudent
                {
                    ClassroomId = classrooms[0].Id,
                    StudentId = students[1].Id, // Bob Davis
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                },
                // Enroll Carol Miller (Grade 9) in Grade 9 Science Class
                new ClassroomStudent
                {
                    ClassroomId = classrooms[1].Id,
                    StudentId = students[2].Id, // Carol Miller
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _context.ClassroomStudents.AddRange(enrollments);
            await _context.SaveChangesAsync();
        }
    }
}
