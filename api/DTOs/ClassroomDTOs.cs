using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    // Classroom DTOs
    public class CreateClassroomRequest
    {
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
    }

    public class ClassroomResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class JoinClassroomRequest
    {
        [Required]
        [StringLength(10)]
        public string ClassCode { get; set; } = string.Empty;
    }

    // Assignment DTOs
    public class CreateAssignmentRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Range(0, 1000)]
        public int Points { get; set; }
        
        [Required]
        public int ClassroomId { get; set; }
    }

    public class AssignmentResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int Points { get; set; }
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsSubmitted { get; set; }
        public int? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
    }

    public class SubmitAssignmentRequest
    {
        [Required]
        [StringLength(2000)]
        public string SubmissionText { get; set; } = string.Empty;
    }

    public class GradeAssignmentRequest
    {
        [Range(0, 1000)]
        public int Grade { get; set; }
        
        [StringLength(500)]
        public string? TeacherFeedback { get; set; }
    }

    // Announcement DTOs
    public class CreateAnnouncementRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public int ClassroomId { get; set; }
    }

    public class AnnouncementResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Dashboard DTOs
    public class TeacherDashboardResponse
    {
        public int TotalClassrooms { get; set; }
        public int TotalStudents { get; set; }
        public int PendingAssignments { get; set; }
        public List<ClassroomResponse> RecentClassrooms { get; set; } = new List<ClassroomResponse>();
        public List<AssignmentResponse> RecentAssignments { get; set; } = new List<AssignmentResponse>();
        public List<AnnouncementResponse> RecentAnnouncements { get; set; } = new List<AnnouncementResponse>();
    }

    public class StudentDashboardResponse
    {
        public int EnrolledClasses { get; set; }
        public int PendingAssignments { get; set; }
        public int UpcomingTests { get; set; }
        public List<ClassroomResponse> Classrooms { get; set; } = new List<ClassroomResponse>();
        public List<ClassroomResponse> EnrolledClassrooms { get; set; } = new List<ClassroomResponse>();
        public List<AssignmentResponse> RecentAssignments { get; set; } = new List<AssignmentResponse>();
        public List<AnnouncementResponse> RecentAnnouncements { get; set; } = new List<AnnouncementResponse>();
    }

    // School Head DTOs
    public class CreateSchoolHeadClassroomRequest
    {
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

        [Required]
        public int TeacherId { get; set; }
    }

    public class AssignTeacherRequest
    {
        [Required]
        public int TeacherId { get; set; }
    }

    public class TeacherResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentPhone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
