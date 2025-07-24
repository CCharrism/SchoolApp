using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class SchoolSettingsRequest
    {
        [MaxLength(100)]
        public string SchoolDisplayName { get; set; } = string.Empty;
        
        [MaxLength(500000)] // Increased for base64 image data
        public string LogoImageUrl { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression("^(sidebar|topbar)$", ErrorMessage = "NavigationType must be either 'sidebar' or 'topbar'")]
        public string NavigationType { get; set; } = "sidebar";
        
        [Required]
        [RegularExpression("^[0-9A-Fa-f]{6}$", ErrorMessage = "ThemeColor must be a valid 6-character hex color code")]
        public string ThemeColor { get; set; } = "DD4470";
    }
    
    public class SchoolSettingsResponse
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public string SchoolDisplayName { get; set; } = string.Empty;
        public string LogoImageUrl { get; set; } = string.Empty;
        public string NavigationType { get; set; } = "sidebar";
        public string ThemeColor { get; set; } = "DD4470";
        public DateTime UpdatedAt { get; set; }
    }
}
