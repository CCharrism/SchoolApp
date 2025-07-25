using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.DTOs;
using api.Models;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("school-hierarchy")]
        public async Task<ActionResult<List<SchoolHierarchyDto>>> GetSchoolHierarchy()
        {
            // Get all data in one efficient query with includes
            var schoolsWithData = await _context.Schools
                .Include(s => s.CreatedByAdmin)
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    School = s,
                    Branches = _context.Branches
                        .Where(b => b.SchoolId == s.Id && b.IsActive)
                        .Select(b => new
                        {
                            Branch = b,
                            SchoolHead = _context.Users
                                .Where(u => u.Username == b.SchoolHeadUsername && u.Role == "SchoolHead")
                                .FirstOrDefault(),
                            CourseCount = _context.Courses
                                .Count(c => c.BranchId == b.Id && c.IsActive)
                        })
                        .ToList(),
                    SchoolOwner = _context.Users
                        .Where(u => u.Username == s.OwnerUsername && u.Role == "SchoolOwner")
                        .FirstOrDefault()
                })
                .OrderBy(s => s.School.SchoolName)
                .ToListAsync();

            var hierarchy = new List<SchoolHierarchyDto>();

            foreach (var schoolData in schoolsWithData)
            {
                var schoolHeads = new List<SchoolHeadDto>();
                
                foreach (var branchData in schoolData.Branches)
                {
                    if (branchData.SchoolHead != null)
                    {
                        schoolHeads.Add(new SchoolHeadDto
                        {
                            Id = branchData.SchoolHead.Id,
                            Username = branchData.SchoolHead.Username,
                            BranchId = branchData.Branch.Id,
                            BranchName = branchData.Branch.BranchName,
                            BranchLocation = branchData.Branch.Location,
                            CourseCount = branchData.CourseCount,
                            CreatedAt = branchData.SchoolHead.CreatedAt
                        });
                    }
                }

                hierarchy.Add(new SchoolHierarchyDto
                {
                    SchoolId = schoolData.School.Id,
                    SchoolName = schoolData.School.SchoolName,
                    SchoolOwner = new SchoolOwnerDto
                    {
                        Id = schoolData.SchoolOwner?.Id ?? 0,
                        Name = schoolData.School.OwnerName,
                        Username = schoolData.School.OwnerUsername,
                        Email = schoolData.School.Email,
                        Phone = schoolData.School.Phone,
                        Address = schoolData.School.Address,
                        CreatedAt = schoolData.School.CreatedAt
                    },
                    SchoolHeads = schoolHeads,
                    TotalBranches = schoolData.Branches.Count,
                    TotalCourses = schoolHeads.Sum(sh => sh.CourseCount),
                    CreatedAt = schoolData.School.CreatedAt,
                    CreatedByAdmin = schoolData.School.CreatedByAdmin.Username
                });
            }

            return Ok(hierarchy);
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<AdminStatisticsDto>> GetAdminStatistics()
        {
            var totalSchools = await _context.Schools.CountAsync(s => s.IsActive);
            var totalOwners = await _context.Users.CountAsync(u => u.Role == "SchoolOwner");
            var totalSchoolHeads = await _context.Users.CountAsync(u => u.Role == "SchoolHead");
            var totalBranches = await _context.Branches.CountAsync(b => b.IsActive);
            var totalCourses = await _context.Courses.CountAsync(c => c.IsActive);

            var recentSchools = await _context.Schools
                .Include(s => s.CreatedByAdmin)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new RecentSchoolDto
                {
                    SchoolName = s.SchoolName,
                    OwnerName = s.OwnerName,
                    CreatedAt = s.CreatedAt,
                    CreatedByAdmin = s.CreatedByAdmin.Username
                })
                .ToListAsync();

            return Ok(new AdminStatisticsDto
            {
                TotalSchools = totalSchools,
                TotalOwners = totalOwners,
                TotalSchoolHeads = totalSchoolHeads,
                TotalBranches = totalBranches,
                TotalCourses = totalCourses,
                RecentSchools = recentSchools
            });
        }
    }
}
