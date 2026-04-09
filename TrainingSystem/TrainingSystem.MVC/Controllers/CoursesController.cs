using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.PrerequisiteCourse)
                .ToListAsync();

            return View(courses);
        }

        //  CREATE 
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateCourse(course);

            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(course);
        }

        //  EDIT 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            LoadDropdowns();
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Course course)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateCourse(course); 

            if (ModelState.IsValid)
            {
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(course);
        }

        // DELETE 
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var course = await _context.Courses.FindAsync(id);

            if (course != null) 
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //  VALIDATION 
        private void ValidateCourse(Course course)
        {
            if (course.DurationHours <= 0)
                ModelState.AddModelError("", "Duration must be greater than 0");

            if (course.DefaultCapacity <= 0)
                ModelState.AddModelError("", "Capacity must be greater than 0");

            if (course.EnrollmentFee < 0)
                ModelState.AddModelError("", "Fee cannot be negative");
        }

        private void LoadDropdowns()
        {
            ViewBag.Categories = new SelectList(_context.SubjectCategories, "CategoryId", "CategoryName");

            ViewBag.Courses = new SelectList(_context.Courses, "CourseId", "Title");
        }
    }
}