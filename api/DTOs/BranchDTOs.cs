using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateBranchDto
    {
        [Required]
        [MaxLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string SchoolHeadUsername { get; set; } = string.Empty;
        
        [Required]
        public string SchoolHeadPassword { get; set; } = string.Empty;
    }
    
    public class BranchDto
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolHeadUsername { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CourseCount { get; set; }
    }
    
    public class UpdateBranchDto
    {
        [Required]
        [MaxLength(100)]
        public string BranchName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}
