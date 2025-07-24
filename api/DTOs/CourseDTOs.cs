using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateCourseDto
    {
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public string? ThumbnailImageUrl { get; set; }
    }
    
    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailImageUrl { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LessonCount { get; set; }
        public List<CourseLessonDto> Lessons { get; set; } = new List<CourseLessonDto>();
    }
    
    public class UpdateCourseDto
    {
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public string? ThumbnailImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class CreateCourseLessonDto
    {
        [Required]
        [MaxLength(100)]
        public string LessonTitle { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string YouTubeUrl { get; set; } = string.Empty;
        
        public int SortOrder { get; set; } = 0;
    }
    
    public class CourseLessonDto
    {
        public int Id { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string YouTubeUrl { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class UpdateCourseLessonDto
    {
        [Required]
        [MaxLength(100)]
        public string LessonTitle { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string YouTubeUrl { get; set; } = string.Empty;
        
        public int SortOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
    }
}
