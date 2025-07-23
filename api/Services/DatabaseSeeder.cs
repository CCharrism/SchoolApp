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
            var adminExists = await _context.Users.AnyAsync(u => u.Username == "admin");
            
            if (!adminExists)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
