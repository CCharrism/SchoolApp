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
            
            // Update existing users to have IsActive = true if not set
            await UpdateExistingUsersIsActive();
            
            // Check if admin user already exists
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    IsActive = true,
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
                    IsActive = true,
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
                        IsActive = true,
                        CreatedAt = school.CreatedAt
                    };

                    _context.Users.Add(schoolOwnerUser);
                }
            }

            await _context.SaveChangesAsync();
        }
        
        private async Task UpdateExistingUsersIsActive()
        {
            // Update all existing users to have IsActive = true
            var usersToUpdate = await _context.Users
                .Where(u => !u.IsActive) // This will also catch users where IsActive is null/false
                .ToListAsync();
                
            foreach (var user in usersToUpdate)
            {
                user.IsActive = true;
            }
            
            if (usersToUpdate.Any())
            {
                await _context.SaveChangesAsync();
            }
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
    }
}
