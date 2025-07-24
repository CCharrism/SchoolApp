using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;
using api.Models;
using api.DTOs;
using BCrypt.Net;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BranchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranches()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userRole == "SchoolOwner")
            {
                var school = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == username);

                if (school == null)
                {
                    return NotFound("School not found for this owner");
                }

                var branches = await _context.Branches
                    .Where(b => b.SchoolId == school.Id)
                    .Include(b => b.School)
                    .Include(b => b.Courses)
                    .Select(b => new BranchDto
                    {
                        Id = b.Id,
                        BranchName = b.BranchName,
                        Description = b.Description,
                        Location = b.Location,
                        SchoolId = b.SchoolId,
                        SchoolName = b.School.SchoolName,
                        SchoolHeadUsername = b.SchoolHeadUsername,
                        IsActive = b.IsActive,
                        CreatedAt = b.CreatedAt,
                        CourseCount = b.Courses.Count(c => c.IsActive)
                    })
                    .ToListAsync();

                return Ok(branches);
            }

            return Forbid("Only school owners can view branches");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BranchDto>> GetBranch(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var branch = await _context.Branches
                .Include(b => b.School)
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return NotFound("Branch not found");
            }

            // Check permissions
            if (userRole == "SchoolOwner")
            {
                var school = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == username);
                
                if (school == null || branch.SchoolId != school.Id)
                {
                    return Forbid("You don't have permission to view this branch");
                }
            }
            else if (userRole == "SchoolHead")
            {
                if (branch.SchoolHeadUsername != username)
                {
                    return Forbid("You don't have permission to view this branch");
                }
            }
            else
            {
                return Forbid("Insufficient permissions");
            }

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                BranchName = branch.BranchName,
                Description = branch.Description,
                Location = branch.Location,
                SchoolId = branch.SchoolId,
                SchoolName = branch.School.SchoolName,
                SchoolHeadUsername = branch.SchoolHeadUsername,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt,
                CourseCount = branch.Courses.Count(c => c.IsActive)
            };

            return Ok(branchDto);
        }

        [HttpPost]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<ActionResult<BranchDto>> CreateBranch(CreateBranchDto createBranchDto)
        {
            var ownerUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.OwnerUsername == ownerUsername);

            if (school == null)
            {
                return NotFound("School not found for this owner");
            }

            // Check if school head username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == createBranchDto.SchoolHeadUsername);

            if (existingUser != null)
            {
                return BadRequest("Username already exists");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Create school head user
                var schoolHead = new User
                {
                    Username = createBranchDto.SchoolHeadUsername,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createBranchDto.SchoolHeadPassword),
                    Role = "SchoolHead",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(schoolHead);
                await _context.SaveChangesAsync();

                // Create branch
                var branch = new Branch
                {
                    BranchName = createBranchDto.BranchName,
                    Description = createBranchDto.Description,
                    Location = createBranchDto.Location,
                    SchoolId = school.Id,
                    SchoolHeadUsername = createBranchDto.SchoolHeadUsername,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var branchDto = new BranchDto
                {
                    Id = branch.Id,
                    BranchName = branch.BranchName,
                    Description = branch.Description,
                    Location = branch.Location,
                    SchoolId = branch.SchoolId,
                    SchoolName = school.SchoolName,
                    SchoolHeadUsername = branch.SchoolHeadUsername,
                    IsActive = branch.IsActive,
                    CreatedAt = branch.CreatedAt,
                    CourseCount = 0
                };

                return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, branchDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while creating the branch");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> UpdateBranch(int id, UpdateBranchDto updateBranchDto)
        {
            var ownerUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.OwnerUsername == ownerUsername);

            if (school == null)
            {
                return NotFound("School not found for this owner");
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == school.Id);

            if (branch == null)
            {
                return NotFound("Branch not found");
            }

            branch.BranchName = updateBranchDto.BranchName;
            branch.Description = updateBranchDto.Description;
            branch.Location = updateBranchDto.Location;
            branch.IsActive = updateBranchDto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var ownerUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var school = await _context.Schools
                .FirstOrDefaultAsync(s => s.OwnerUsername == ownerUsername);

            if (school == null)
            {
                return NotFound("School not found for this owner");
            }

            var branch = await _context.Branches
                .Include(b => b.Courses)
                .FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == school.Id);

            if (branch == null)
            {
                return NotFound("Branch not found");
            }

            // Check if branch has active courses
            if (branch.Courses.Any(c => c.IsActive))
            {
                return BadRequest("Cannot delete branch with active courses. Please deactivate or delete all courses first.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Remove school head user
                var schoolHead = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == branch.SchoolHeadUsername);

                if (schoolHead != null)
                {
                    _context.Users.Remove(schoolHead);
                }

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while deleting the branch");
            }
        }
    }
}
