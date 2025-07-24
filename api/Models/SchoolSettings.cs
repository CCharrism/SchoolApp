using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class SchoolSettings
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SchoolId { get; set; }
        
        [ForeignKey("SchoolId")]
        public virtual School School { get; set; } = null!;
        
        [MaxLength(100)]
        public string SchoolDisplayName { get; set; } = string.Empty;
        
        [MaxLength(500000)] // Increased for base64 image data
        public string LogoImageUrl { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string NavigationType { get; set; } = "sidebar"; // "sidebar" or "topbar"
        
        [MaxLength(10)]
        public string ThemeColor { get; set; } = "DD4470"; // Default color without #
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
