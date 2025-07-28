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
        public UserInfo? User { get; set; } // User information for the client
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? SchoolName { get; set; }
    }

    public class UpdateStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CreateTeacherRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string Subject { get; set; } = string.Empty;
        
        public string Qualification { get; set; } = string.Empty;
    }

    public class CreateStudentRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        public string Grade { get; set; } = string.Empty;
        
        public string RollNumber { get; set; } = string.Empty;
        
        public string ParentName { get; set; } = string.Empty;
        
        public string ParentPhone { get; set; } = string.Empty;
    }
}
