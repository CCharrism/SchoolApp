using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class CourseLesson
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LessonTitle { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string YouTubeUrl { get; set; } = string.Empty;
        
        [Required]
        public int CourseId { get; set; }
        
        [ForeignKey("CourseId")]
        public Course Course { get; set; } = null!;
        
        public int SortOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
