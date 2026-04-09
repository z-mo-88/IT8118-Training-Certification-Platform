using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class SessionEnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;

        public SessionEnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        // VIEW STUDENTS
        public async Task<IActionResult> Index(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                .ThenInclude(s => s.Course)
                .Where(e => e.SessionId == id)
                .ToListAsync();

            ViewBag.SessionId = id;

            return View(enrollments);
        }

        // MARK ATTENDING
        public async Task<IActionResult> MarkAttending(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();

            enrollment.Status = "Attending";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        }

        //RECORD RESULT
        [HttpPost]
        public async Task<IActionResult> RecordResult(int enrollmentId, bool isPassed, string remarks)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null)
                return NotFound();

            //Update status
            enrollment.Status = "Completed";

            var result = new AssessmentResult
            {
                EnrollmentId = enrollmentId,
                IsPassed = isPassed,
                Remarks = remarks,
                RecordDate = DateOnly.FromDateTime(DateTime.Now),
                RecordTime = TimeOnly.FromDateTime(DateTime.Now)
            };

            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();

            //NOTIFICATION (PASS/FAIL) 
            string message = isPassed
                ? "You passed the course"
                : "You did not pass the course";

            await new NotificationController(_context)
                .CreateNotification(enrollment.UserId, message);



           //CERTIFICATION PROGRESS 
            if (isPassed)
            {
                int userId = enrollment.UserId;
                int courseId = enrollment.Session.CourseId;

                var trackCourses = await _context.CertificationTrackCourses
                    .Where(t => t.CourseId == courseId)
                    .ToListAsync();

                foreach (var track in trackCourses)
                {
                    int trackId = track.CertificationTrackId;

                    var requiredCourses = await _context.CertificationTrackCourses
                        .Where(t => t.CertificationTrackId == trackId)
                        .Select(t => t.CourseId)
                        .ToListAsync();

                    var passedCourses = await _context.AssessmentResults
                        .Include(a => a.Enrollment)
                        .ThenInclude(e => e.Session)
                        .Where(a => a.Enrollment.UserId == userId && a.IsPassed)
                        .Select(a => a.Enrollment.Session.CourseId)
                        .Distinct()
                        .ToListAsync();

                    bool completed = requiredCourses.All(rc => passedCourses.Contains(rc));

                    var progress = await _context.TraineeCertificationProgresses
                        .FirstOrDefaultAsync(p =>
                            p.UserId == userId &&
                            p.CertificationTrackId == trackId);

                    int percent = requiredCourses.Count == 0
                        ? 0
                        : (int)((double)passedCourses.Count / requiredCourses.Count * 100);

                    if (progress == null)
                    {
                        progress = new TraineeCertificationProgress
                        {
                            UserId = userId,
                            CertificationTrackId = trackId,
                            Status = completed ? "Eligible" : "In Progress",
                            ProgressPercent = percent,
                            EligibleDate = DateOnly.FromDateTime(DateTime.Now)
                        };

                        _context.TraineeCertificationProgresses.Add(progress);
                    }
                    else
                    {
                        progress.Status = completed ? "Eligible" : "In Progress";
                        progress.ProgressPercent = percent;

                        if (completed)
                            progress.EligibleDate = DateOnly.FromDateTime(DateTime.Now);
                    }

                    await _context.SaveChangesAsync();

                    //  Notify ONLY when eligible
                    if (completed)
                    {
                        await new NotificationController(_context)
                            .CreateNotification(userId, "You are now eligible for certification");
                    }
                }
            }
  
            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        }
    }
}