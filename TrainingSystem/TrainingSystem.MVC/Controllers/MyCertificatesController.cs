using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class MyCertificatesController : BaseController
    {
        private readonly AppDbContext _context;

        public MyCertificatesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (UserId == null)
                return RedirectToAction("Login", "Account");

            int userId = UserId.Value;

            var tracks = await _context.CertificationTracks.ToListAsync();

            var result = new List<TraineeCertificationProgress>();
            var missingCoursesDict = new Dictionary<int, int>();

            foreach (var track in tracks)
            {
                int trackId = track.CertificationTrackId;

                // Required courses
                var requiredCourses = await _context.CertificationTrackCourses
                    .Where(t => t.CertificationTrackId == trackId && t.IsRequired)
                    .Select(t => t.CourseId)
                    .ToListAsync();

                
                if (!requiredCourses.Any())
                {
                    requiredCourses = await _context.CertificationTrackCourses
                        .Where(t => t.CertificationTrackId == trackId)
                        .Select(t => t.CourseId)
                        .ToListAsync();
                }

                // Passed courses
                var passedCourses = await _context.AssessmentResults
                    .Include(a => a.Enrollment)
                        .ThenInclude(e => e.Session)
                    .Where(a => a.Enrollment.UserId == userId && a.IsPassed)
                    .Select(a => a.Enrollment.Session.CourseId)
                    .Distinct()
                    .ToListAsync();

                int matched = passedCourses.Count(pc => requiredCourses.Contains(pc));

                int percent = requiredCourses.Count == 0
                    ? 0
                    : (matched * 100) / requiredCourses.Count;

                bool completed = requiredCourses.All(rc => passedCourses.Contains(rc));

                int missing = requiredCourses.Count - matched;
                missingCoursesDict[trackId] = missing;

                result.Add(new TraineeCertificationProgress
                {
                    CertificationTrackId = trackId,
                    CertificationTrack = track,
                    UserId = userId,
                    ProgressPercent = percent,
                    Status = completed ? "Eligible" : "In Progress",
                    EligibleDate = completed
                        ? DateOnly.FromDateTime(DateTime.Now)
                        : DateOnly.MinValue
                });
            }

            // Certificates
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.Certificates = certificates;
            ViewBag.MissingCourses = missingCoursesDict;

            return View(result);
        }
    }
}