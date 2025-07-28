using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public int ClassroomId { get; set; }
        
        [ForeignKey("ClassroomId")]
        public virtual Classroom Classroom { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
