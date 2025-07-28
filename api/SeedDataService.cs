using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;

namespace api.Services
{
    public static class SeedDataService
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Add sample school
            if (!context.Schools.Any())
            {
                var school = new School
                {
                    SchoolName = "Sample High School",
                    OwnerName = "School Owner",
                    OwnerUsername = "school.owner",
                    OwnerPasswordHash = BCrypt.Net.BCrypt.HashPassword("owner123"),
                    Address = "123 Education St, Learning City, LC 12345",
                    Phone = "+1234567890",
                    Email = "info@samplehighschool.edu",
                    IsActive = true,
                    CreatedByAdminId = 1
                };
                
                context.Schools.Add(school);
                await context.SaveChangesAsync();
                Console.WriteLine("Added sample school");
            }

            // Add sample teachers
            if (!context.Teachers.Any())
            {
                var teachers = new List<Teacher>
                {
                    new Teacher
                    {
                        FullName = "Alice Smith",
                        Username = "alice.smith",
                        Email = "alice.smith@school.edu",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                        Phone = "+1234567890",
                        Subject = "Mathematics",
                        Qualification = "M.Sc Mathematics",
                        SchoolId = 1,
                        IsActive = true
                    },
                    new Teacher
                    {
                        FullName = "Sarah Johnson",
                        Username = "sarah.johnson",
                        Email = "sarah.johnson@school.edu",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                        Phone = "+1234567891",
                        Subject = "English",
                        Qualification = "M.A English Literature",
                        SchoolId = 1,
                        IsActive = true
                    }
                };
                
                context.Teachers.AddRange(teachers);
                await context.SaveChangesAsync();
                Console.WriteLine("Added sample teachers");
            }
            
            // Add sample students
            if (!context.Students.Any())
            {
                var students = new List<Student>
                {
                    new Student
                    {
                        FullName = "Emma Wilson",
                        Username = "emma.wilson",
                        Email = "emma.wilson@student.edu",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                        Phone = "+1234567893",
                        Grade = "10th Grade",
                        RollNumber = "2024001",
                        ParentName = "Robert Wilson",
                        ParentPhone = "+1234567894",
                        SchoolId = 1,
                        IsActive = true
                    },
                    new Student
                    {
                        FullName = "James Davis",
                        Username = "james.davis",
                        Email = "james.davis@student.edu",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                        Phone = "+1234567895",
                        Grade = "10th Grade",
                        RollNumber = "2024002",
                        ParentName = "Linda Davis",
                        ParentPhone = "+1234567896",
                        SchoolId = 1,
                        IsActive = true
                    }
                };
                
                context.Students.AddRange(students);
                await context.SaveChangesAsync();
                Console.WriteLine("Added sample students");
            }
            
            // Add sample classrooms
            if (!context.Classrooms.Any())
            {
                var teacher = context.Teachers.First();
                var classrooms = new List<Classroom>
                {
                    new Classroom
                    {
                        Name = "Math 101",
                        Description = "Basic Mathematics for 10th Grade",
                        Subject = "Mathematics",
                        Section = "A",
                        ClassCode = "MATH101A",
                        TeacherId = teacher.Id,
                        SchoolId = 1,
                        IsActive = true
                    },
                    new Classroom
                    {
                        Name = "English Literature",
                        Description = "English Literature for 11th Grade",
                        Subject = "English",
                        Section = "B", 
                        ClassCode = "ENG11B",
                        TeacherId = teacher.Id,
                        SchoolId = 1,
                        IsActive = true
                    }
                };
                
                context.Classrooms.AddRange(classrooms);
                await context.SaveChangesAsync();
                Console.WriteLine("Added sample classrooms");
            }

            Console.WriteLine("Sample data seeding completed!");
        }
    }
}
