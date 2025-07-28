using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Classroom
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Section { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string ClassCode { get; set; } = string.Empty;
        
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;
        
        public int SchoolId { get; set; }
        public School School { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<ClassroomStudent> ClassroomStudents { get; set; } = new List<ClassroomStudent>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    }

    public class ClassroomStudent
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;
        
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Assignment
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime DueDate { get; set; }
        public int Points { get; set; }
        
        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();
    }

    public class StudentAssignment
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;
        
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        
        public DateTime? SubmittedAt { get; set; }
        public string? SubmissionText { get; set; }
        public int? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
