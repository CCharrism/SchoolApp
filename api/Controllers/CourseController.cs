using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;
using api.Models;
using api.DTOs;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userRole == "SchoolHead")
            {
                var branch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.SchoolHeadUsername == username);

                if (branch == null)
                {
                    return NotFound("Branch not found for this school head");
                }

                var courses = await _context.Courses
                    .Where(c => c.BranchId == branch.Id)
                    .Include(c => c.Branch)
                    .Include(c => c.Lessons)
                    .Select(c => new CourseDto
                    {
                        Id = c.Id,
                        CourseName = c.CourseName,
                        Description = c.Description,
                        ThumbnailImageUrl = c.ThumbnailImageUrl,
                        BranchId = c.BranchId,
                        BranchName = c.Branch.BranchName,
                        CreatedBy = c.CreatedBy,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        LessonCount = c.Lessons.Count(l => l.IsActive),
                        Lessons = c.Lessons.Where(l => l.IsActive)
                            .OrderBy(l => l.SortOrder)
                            .Select(l => new CourseLessonDto
                            {
                                Id = l.Id,
                                LessonTitle = l.LessonTitle,
                                Description = l.Description,
                                YouTubeUrl = l.YouTubeUrl,
                                CourseId = l.CourseId,
                                SortOrder = l.SortOrder,
                                IsActive = l.IsActive,
                                CreatedAt = l.CreatedAt
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(courses);
            }
            else if (userRole == "SchoolOwner")
            {
                var school = await _context.Schools
                    .FirstOrDefaultAsync(s => s.OwnerUsername == username);

                if (school == null)
                {
                    return NotFound("School not found for this owner");
                }

                var courses = await _context.Courses
                    .Where(c => c.Branch.SchoolId == school.Id)
                    .Include(c => c.Branch)
                    .Include(c => c.Lessons)
                    .Select(c => new CourseDto
                    {
                        Id = c.Id,
                        CourseName = c.CourseName,
                        Description = c.Description,
                        ThumbnailImageUrl = c.ThumbnailImageUrl,
                        BranchId = c.BranchId,
                        BranchName = c.Branch.BranchName,
                        CreatedBy = c.CreatedBy,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        LessonCount = c.Lessons.Count(l => l.IsActive),
                        Lessons = c.Lessons.Where(l => l.IsActive)
                            .OrderBy(l => l.SortOrder)
                            .Select(l => new CourseLessonDto
                            {
                                Id = l.Id,
                                LessonTitle = l.LessonTitle,
                                Description = l.Description,
                                YouTubeUrl = l.YouTubeUrl,
                                CourseId = l.CourseId,
                                SortOrder = l.SortOrder,
                                IsActive = l.IsActive,
                                CreatedAt = l.CreatedAt
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(courses);
            }

            return Forbid("Insufficient permissions");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var course = await _context.Courses
                .Include(c => c.Branch)
                .ThenInclude(b => b.School)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Course not found");
            }

            // Check permissions
            if (userRole == "SchoolHead")
            {
                if (course.Branch.SchoolHeadUsername != username)
                {
                    return Forbid("You don't have permission to view this course");
                }
            }
            else if (userRole == "SchoolOwner")
            {
                if (course.Branch.School.OwnerUsername != username)
                {
                    return Forbid("You don't have permission to view this course");
                }
            }
            else
            {
                return Forbid("Insufficient permissions");
            }

            var courseDto = new CourseDto
            {
                Id = course.Id,
                CourseName = course.CourseName,
                Description = course.Description,
                ThumbnailImageUrl = course.ThumbnailImageUrl,
                BranchId = course.BranchId,
                BranchName = course.Branch.BranchName,
                CreatedBy = course.CreatedBy,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                LessonCount = course.Lessons.Count(l => l.IsActive),
                Lessons = course.Lessons.Where(l => l.IsActive)
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new CourseLessonDto
                    {
                        Id = l.Id,
                        LessonTitle = l.LessonTitle,
                        Description = l.Description,
                        YouTubeUrl = l.YouTubeUrl,
                        CourseId = l.CourseId,
                        SortOrder = l.SortOrder,
                        IsActive = l.IsActive,
                        CreatedAt = l.CreatedAt
                    }).ToList()
            };

            return Ok(courseDto);
        }

        [HttpPost]
        [Authorize(Roles = "SchoolHead")]
        public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createCourseDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var branch = await _context.Branches
                .Include(b => b.School)
                .FirstOrDefaultAsync(b => b.SchoolHeadUsername == username);

            if (branch == null)
            {
                return NotFound("Branch not found for this school head");
            }

            var course = new Course
            {
                CourseName = createCourseDto.CourseName,
                Description = createCourseDto.Description,
                ThumbnailImageUrl = createCourseDto.ThumbnailImageUrl,
                BranchId = branch.Id,
                CreatedBy = username!,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var courseDto = new CourseDto
            {
                Id = course.Id,
                CourseName = course.CourseName,
                Description = course.Description,
                ThumbnailImageUrl = course.ThumbnailImageUrl,
                BranchId = course.BranchId,
                BranchName = branch.BranchName,
                CreatedBy = course.CreatedBy,
                IsActive = course.IsActive,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                LessonCount = 0,
                Lessons = new List<CourseLessonDto>()
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, courseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SchoolHead")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto updateCourseDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var course = await _context.Courses
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Course not found");
            }

            if (course.Branch.SchoolHeadUsername != username)
            {
                return Forbid("You don't have permission to update this course");
            }

            course.CourseName = updateCourseDto.CourseName;
            course.Description = updateCourseDto.Description;
            course.ThumbnailImageUrl = updateCourseDto.ThumbnailImageUrl;
            course.IsActive = updateCourseDto.IsActive;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SchoolHead")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var course = await _context.Courses
                .Include(c => c.Branch)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Course not found");
            }

            if (course.Branch.SchoolHeadUsername != username)
            {
                return Forbid("You don't have permission to delete this course");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Course Lesson endpoints
        [HttpPost("{courseId}/lessons")]
        [Authorize(Roles = "SchoolHead")]
        public async Task<ActionResult<CourseLessonDto>> CreateLesson(int courseId, CreateCourseLessonDto createLessonDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var course = await _context.Courses
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found");
            }

            if (course.Branch.SchoolHeadUsername != username)
            {
                return Forbid("You don't have permission to add lessons to this course");
            }

            var lesson = new CourseLesson
            {
                LessonTitle = createLessonDto.LessonTitle,
                Description = createLessonDto.Description,
                YouTubeUrl = createLessonDto.YouTubeUrl,
                CourseId = courseId,
                SortOrder = createLessonDto.SortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseLessons.Add(lesson);
            await _context.SaveChangesAsync();

            var lessonDto = new CourseLessonDto
            {
                Id = lesson.Id,
                LessonTitle = lesson.LessonTitle,
                Description = lesson.Description,
                YouTubeUrl = lesson.YouTubeUrl,
                CourseId = lesson.CourseId,
                SortOrder = lesson.SortOrder,
                IsActive = lesson.IsActive,
                CreatedAt = lesson.CreatedAt
            };

            return CreatedAtAction(nameof(GetLesson), new { courseId = courseId, lessonId = lesson.Id }, lessonDto);
        }

        [HttpGet("{courseId}/lessons/{lessonId}")]
        public async Task<ActionResult<CourseLessonDto>> GetLesson(int courseId, int lessonId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            var lesson = await _context.CourseLessons
                .Include(l => l.Course)
                .ThenInclude(c => c.Branch)
                .ThenInclude(b => b.School)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId);

            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            // Check permissions
            if (userRole == "SchoolHead")
            {
                if (lesson.Course.Branch.SchoolHeadUsername != username)
                {
                    return Forbid("You don't have permission to view this lesson");
                }
            }
            else if (userRole == "SchoolOwner")
            {
                if (lesson.Course.Branch.School.OwnerUsername != username)
                {
                    return Forbid("You don't have permission to view this lesson");
                }
            }
            else
            {
                return Forbid("Insufficient permissions");
            }

            var lessonDto = new CourseLessonDto
            {
                Id = lesson.Id,
                LessonTitle = lesson.LessonTitle,
                Description = lesson.Description,
                YouTubeUrl = lesson.YouTubeUrl,
                CourseId = lesson.CourseId,
                SortOrder = lesson.SortOrder,
                IsActive = lesson.IsActive,
                CreatedAt = lesson.CreatedAt
            };

            return Ok(lessonDto);
        }

        [HttpPut("{courseId}/lessons/{lessonId}")]
        [Authorize(Roles = "SchoolHead")]
        public async Task<IActionResult> UpdateLesson(int courseId, int lessonId, UpdateCourseLessonDto updateLessonDto)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var lesson = await _context.CourseLessons
                .Include(l => l.Course)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId);

            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            if (lesson.Course.Branch.SchoolHeadUsername != username)
            {
                return Forbid("You don't have permission to update this lesson");
            }

            lesson.LessonTitle = updateLessonDto.LessonTitle;
            lesson.Description = updateLessonDto.Description;
            lesson.YouTubeUrl = updateLessonDto.YouTubeUrl;
            lesson.SortOrder = updateLessonDto.SortOrder;
            lesson.IsActive = updateLessonDto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{courseId}/lessons/{lessonId}")]
        [Authorize(Roles = "SchoolHead")]
        public async Task<IActionResult> DeleteLesson(int courseId, int lessonId)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            
            var lesson = await _context.CourseLessons
                .Include(l => l.Course)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseId == courseId);

            if (lesson == null)
            {
                return NotFound("Lesson not found");
            }

            if (lesson.Course.Branch.SchoolHeadUsername != username)
            {
                return Forbid("You don't have permission to delete this lesson");
            }

            _context.CourseLessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
