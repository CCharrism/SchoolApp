using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? SchoolName { get; set; } // Optional field for school owners and heads
        public string? BranchName { get; set; } // Optional field for school heads
    }
}
