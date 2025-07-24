using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(500000)] // For base64 image
        public string? ThumbnailImageUrl { get; set; }
        
        [Required]
        public int BranchId { get; set; }
        
        [ForeignKey("BranchId")]
        public Branch Branch { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty; // School head username
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<CourseLesson> Lessons { get; set; } = new List<CourseLesson>();
    }
}
