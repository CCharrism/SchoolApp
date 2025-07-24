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

            // Check if demo school owner already exists
            var schoolOwnerExists = await _context.Schools.AnyAsync(s => s.OwnerUsername == "owner1");
            
            if (!schoolOwnerExists)
            {
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
            }
        }
    }
}
