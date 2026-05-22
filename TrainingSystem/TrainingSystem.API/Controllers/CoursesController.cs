using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.API.DTOs;

namespace TrainingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "3")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    DurationHours = c.DurationHours,
                    DefaultCapacity = c.DefaultCapacity,
                    EnrollmentFee = c.EnrollmentFee,
                    CategoryId = c.CategoryId,
                    PrerequisiteCourseId = c.PrerequisiteCourseId
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Where(c => c.CourseId == id)
                .Select(c => new CourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    DurationHours = c.DurationHours,
                    DefaultCapacity = c.DefaultCapacity,
                    EnrollmentFee = c.EnrollmentFee,
                    CategoryId = c.CategoryId,
                    PrerequisiteCourseId = c.PrerequisiteCourseId
                })
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto courseDto)
        {
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                DurationHours = courseDto.DurationHours,
                DefaultCapacity = courseDto.DefaultCapacity,
                EnrollmentFee = courseDto.EnrollmentFee,
                CategoryId = courseDto.CategoryId,
                PrerequisiteCourseId = courseDto.PrerequisiteCourseId
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var result = new CourseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                DurationHours = course.DurationHours,
                DefaultCapacity = course.DefaultCapacity,
                EnrollmentFee = course.EnrollmentFee,
                CategoryId = course.CategoryId,
                PrerequisiteCourseId = course.PrerequisiteCourseId
            };

            return CreatedAtAction(nameof(GetCourseById), new { id = course.CourseId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto updatedCourse)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.Title = updatedCourse.Title;
            course.Description = updatedCourse.Description;
            course.DurationHours = updatedCourse.DurationHours;
            course.DefaultCapacity = updatedCourse.DefaultCapacity;
            course.EnrollmentFee = updatedCourse.EnrollmentFee;
            course.CategoryId = updatedCourse.CategoryId;
            course.PrerequisiteCourseId = updatedCourse.PrerequisiteCourseId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchCourse(int id, PatchCourseDto patchCourse)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            if (patchCourse.Title != null)
                course.Title = patchCourse.Title;

            if (patchCourse.Description != null)
                course.Description = patchCourse.Description;

            if (patchCourse.DurationHours.HasValue)
                course.DurationHours = patchCourse.DurationHours.Value;

            if (patchCourse.DefaultCapacity.HasValue)
                course.DefaultCapacity = patchCourse.DefaultCapacity.Value;

            if (patchCourse.EnrollmentFee.HasValue)
                course.EnrollmentFee = patchCourse.EnrollmentFee.Value;

            if (patchCourse.CategoryId.HasValue)
                course.CategoryId = patchCourse.CategoryId.Value;

            if (patchCourse.PrerequisiteCourseId.HasValue)
                course.PrerequisiteCourseId = patchCourse.PrerequisiteCourseId.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}