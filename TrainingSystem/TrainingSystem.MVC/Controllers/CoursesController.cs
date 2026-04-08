using Microsoft.AspNetCore.Mvc;
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
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            var courses = await _context.Courses.ToListAsync();

            var enrollmentCounts = await _context.CourseSessions
                .Select(cs => new
                {
                    cs.CourseId,
                    Count = cs.Enrollments.Count()
                })
                .GroupBy(x => x.CourseId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(x => x.Count)
                );

            ViewBag.EnrollmentCounts = enrollmentCounts;

            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Category");
            ModelState.Remove("CourseSessions");
            ModelState.Remove("Enrollments");
            ModelState.Remove("CertificationTrackCourses");
            ModelState.Remove("CourseEquipmentRequirements");
            ModelState.Remove("InversePrerequisiteCourse");
            ModelState.Remove("PrerequisiteCourse");

            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Course course)
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Category");
            ModelState.Remove("CourseSessions");
            ModelState.Remove("Enrollments");
            ModelState.Remove("CertificationTrackCourses");
            ModelState.Remove("CourseEquipmentRequirements");
            ModelState.Remove("InversePrerequisiteCourse");
            ModelState.Remove("PrerequisiteCourse");

            if (ModelState.IsValid)
            {
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (RoleId != 3)
                return RedirectToAction("Login", "Account");

            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}