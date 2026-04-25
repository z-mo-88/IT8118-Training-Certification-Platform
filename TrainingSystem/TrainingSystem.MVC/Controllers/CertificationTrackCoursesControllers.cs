using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificationTrackCoursesController : Controller
    {
        private readonly AppDbContext _context;

        public CertificationTrackCoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var data = await _context.CertificationTrackCourses
                .Include(t => t.CertificationTrack)
                .Include(t => t.Course)
                .ToListAsync();

            return View(data);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Tracks = _context.CertificationTracks.ToList();
            ViewBag.Courses = _context.Courses.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int trackId, int courseId)
        {
            bool isRequired = Request.Form.ContainsKey("isRequired");

            var exists = await _context.CertificationTrackCourses
                .AnyAsync(x => x.CertificationTrackId == trackId && x.CourseId == courseId);

            if (!exists)
            {
                _context.CertificationTrackCourses.Add(new CertificationTrackCourse
                {
                    CertificationTrackId = trackId,
                    CourseId = courseId,
                    IsRequired = isRequired
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        [HttpGet]
        public async Task<IActionResult> Edit(int trackId, int courseId)
        {
            var item = await _context.CertificationTrackCourses
    .Include(x => x.CertificationTrack)
    .Include(x => x.Course)
    .FirstOrDefaultAsync(x =>
        x.CertificationTrackId == trackId &&
        x.CourseId == courseId);

            if (item == null) return NotFound();

            ViewBag.Tracks = _context.CertificationTracks.ToList();
            ViewBag.Courses = _context.Courses.ToList();

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int trackId, int courseId, int newCourseId)
        {
            bool isRequired = Request.Form.ContainsKey("isRequired");

            var item = await _context.CertificationTrackCourses
                .FirstOrDefaultAsync(x =>
                    x.CertificationTrackId == trackId &&
                    x.CourseId == courseId);

            if (item == null) return NotFound();

            bool exists = await _context.CertificationTrackCourses
                .AnyAsync(x =>
                    x.CertificationTrackId == trackId &&
                    x.CourseId == newCourseId &&
                    x.CourseId != courseId);

            if (exists)
            {
                ModelState.AddModelError("", "Course already exists in this track");
                ViewBag.Tracks = _context.CertificationTracks.ToList();
                ViewBag.Courses = _context.Courses.ToList();
                return View(item);
            }

            item.CourseId = newCourseId;
            item.IsRequired = isRequired;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        [HttpGet]
        public async Task<IActionResult> Delete(int trackId, int courseId)
        {
            var item = await _context.CertificationTrackCourses
                .Include(x => x.CertificationTrack)
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x =>
                    x.CertificationTrackId == trackId &&
                    x.CourseId == courseId);

            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int trackId, int courseId)
        {
            var item = await _context.CertificationTrackCourses
                .FirstOrDefaultAsync(x =>
                    x.CertificationTrackId == trackId &&
                    x.CourseId == courseId);

            if (item != null)
            {
                _context.CertificationTrackCourses.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}