using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        public int SchoolId { get; set; }
        
        [ForeignKey("SchoolId")]
        public School School { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string SchoolHeadUsername { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
