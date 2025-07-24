using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class School
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string SchoolName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string OwnerName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string OwnerUsername { get; set; } = string.Empty;
        
        [Required]
        public string OwnerPasswordHash { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int CreatedByAdminId { get; set; }
        
        [ForeignKey("CreatedByAdminId")]
        public virtual User CreatedByAdmin { get; set; } = null!;
    }
}
