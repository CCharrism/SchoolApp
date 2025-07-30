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
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomStudent> ClassroomStudents { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<StudentAssignment> StudentAssignments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        
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
            
            // Configure Teacher entity
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasIndex(t => t.Username).IsUnique();
                entity.HasIndex(t => t.Email).IsUnique();
                entity.HasOne(t => t.School)
                      .WithMany()
                      .HasForeignKey(t => t.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Student entity
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.Username).IsUnique();
                entity.HasIndex(s => s.Email).IsUnique();
                entity.HasOne(s => s.School)
                      .WithMany()
                      .HasForeignKey(s => s.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure Classroom entity
            modelBuilder.Entity<Classroom>(entity =>
            {
                entity.HasIndex(c => c.ClassCode).IsUnique();
                entity.HasOne(c => c.School)
                      .WithMany()
                      .HasForeignKey(c => c.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(c => c.Teacher)
                      .WithMany()
                      .HasForeignKey(c => c.TeacherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configure ClassroomStudent entity
            modelBuilder.Entity<ClassroomStudent>(entity =>
            {
                entity.HasOne(cs => cs.Classroom)
                      .WithMany(c => c.ClassroomStudents)
                      .HasForeignKey(cs => cs.ClassroomId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(cs => cs.Student)
                      .WithMany()
                      .HasForeignKey(cs => cs.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                // Ensure unique enrollment (one student per classroom)
                entity.HasIndex(cs => new { cs.ClassroomId, cs.StudentId })
                      .IsUnique();
            });
            
            // Configure Assignment entity
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasOne(a => a.Classroom)
                      .WithMany(c => c.Assignments)
                      .HasForeignKey(a => a.ClassroomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure StudentAssignment entity
            modelBuilder.Entity<StudentAssignment>(entity =>
            {
                entity.HasOne(sa => sa.Assignment)
                      .WithMany(a => a.StudentAssignments)
                      .HasForeignKey(sa => sa.AssignmentId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(sa => sa.Student)
                      .WithMany()
                      .HasForeignKey(sa => sa.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                // Ensure unique submission (one submission per student per assignment)
                entity.HasIndex(sa => new { sa.AssignmentId, sa.StudentId })
                      .IsUnique();
            });
            
            // Configure Announcement entity
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasOne(an => an.Classroom)
                      .WithMany(c => c.Announcements)
                      .HasForeignKey(an => an.ClassroomId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
