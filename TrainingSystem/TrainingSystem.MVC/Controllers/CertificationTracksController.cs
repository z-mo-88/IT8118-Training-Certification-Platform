using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificationTracksController : Controller
    {
        private readonly AppDbContext _context;

        public CertificationTracksController(AppDbContext context)
        {
            _context = context;
        }

        //  CREATE TRACK 
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CertificationTrack track)
        {
            if (!ModelState.IsValid)
                return View(track);

            _context.CertificationTracks.Add(track);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Track created successfully";

            return RedirectToAction(nameof(Create));
        }

        //  ASSIGN COURSE 
        public IActionResult AssignCourse()
        {
            ViewBag.Tracks = _context.CertificationTracks.ToList();
            ViewBag.Courses = _context.Courses.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignCourse(int trackId, int courseId, bool isRequired)
        {
            var exists = await _context.CertificationTrackCourses
                .AnyAsync(x => x.CourseId == courseId && x.CertificationTrackId == trackId);

            if (exists)
            {
                TempData["Error"] = "Course already assigned to this track";
            }
            else
            {
                _context.CertificationTrackCourses.Add(new CertificationTrackCourse
                {
                    CertificationTrackId = trackId,
                    CourseId = courseId,
                    IsRequired = isRequired
                });

                await _context.SaveChangesAsync();

                TempData["Success"] = "Course assigned successfully";
            }

            return RedirectToAction(nameof(AssignCourse));
        }
    }
}