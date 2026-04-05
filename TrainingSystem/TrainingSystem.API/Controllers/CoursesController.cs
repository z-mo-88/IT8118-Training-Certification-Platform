using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Courses.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Course>> CreateCourse(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return Ok(course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, Course updatedCourse)
        {
            if (id != updatedCourse.CourseId)
                return BadRequest();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Title = updatedCourse.Title;
            course.Description = updatedCourse.Description;
            course.DurationHours = updatedCourse.DurationHours;
            course.DefaultCapacity = updatedCourse.DefaultCapacity;
            course.EnrollmentFee = updatedCourse.EnrollmentFee;
            course.CategoryId = updatedCourse.CategoryId;
            course.PrerequisiteCourseId = updatedCourse.PrerequisiteCourseId;

            await _context.SaveChangesAsync();
            return Ok(course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}