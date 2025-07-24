using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<SchoolSettings> SchoolSettings { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
            });
            
            // Configure School entity
            modelBuilder.Entity<School>(entity =>
            {
                entity.HasIndex(e => e.OwnerUsername).IsUnique();
                entity.HasOne(s => s.CreatedByAdmin)
                      .WithMany()
                      .HasForeignKey(s => s.CreatedByAdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure Branch entity
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasOne(b => b.School)
                      .WithMany()
                      .HasForeignKey(b => b.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.Branch)
                      .WithMany(b => b.Courses)
                      .HasForeignKey(c => c.BranchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure CourseLesson entity
            modelBuilder.Entity<CourseLesson>(entity =>
            {
                entity.HasOne(cl => cl.Course)
                      .WithMany(c => c.Lessons)
                      .HasForeignKey(cl => cl.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
