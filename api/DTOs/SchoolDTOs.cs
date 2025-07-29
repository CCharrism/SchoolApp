using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class CreateSchoolRequest
    {
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
        [MinLength(6)]
        public string OwnerPassword { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
    
    public class SchoolResponse
    {
        public int Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByAdmin { get; set; } = string.Empty;
    }

    public class BulkImportResponse
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
