using System.ComponentModel.DataAnnotations;

namespace api.DTOs
{
    public class SchoolHierarchyDto
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public SchoolOwnerDto SchoolOwner { get; set; } = new SchoolOwnerDto();
        public List<SchoolHeadDto> SchoolHeads { get; set; } = new List<SchoolHeadDto>();
        public int TotalBranches { get; set; }
        public int TotalCourses { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByAdmin { get; set; } = string.Empty;
    }

    public class SchoolOwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class SchoolHeadDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchLocation { get; set; } = string.Empty;
        public int CourseCount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminStatisticsDto
    {
        public int TotalSchools { get; set; }
        public int TotalOwners { get; set; }
        public int TotalSchoolHeads { get; set; }
        public int TotalBranches { get; set; }
        public int TotalCourses { get; set; }
        public List<RecentSchoolDto> RecentSchools { get; set; } = new List<RecentSchoolDto>();
    }

    public class RecentSchoolDto
    {
        public string SchoolName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedByAdmin { get; set; } = string.Empty;
    }
}
