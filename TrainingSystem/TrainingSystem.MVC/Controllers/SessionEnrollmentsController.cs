using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class SessionEnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

        public SessionEnrollmentsController(AppDbContext context)
        {
            _context = context;
            _notification = new NotificationService(_context);
        }

        // ================= VIEW STUDENTS =================
        public async Task<IActionResult> Index(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.AssessmentResults)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .Where(e => e.SessionId == id && e.Status != "Dropped")
                .ToListAsync();

            ViewBag.SessionId = id;

            return View(enrollments);
        }

        // ================= MARK ATTENDING =================
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

        // ================= RECORD RESULT =================
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


            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["Error"] = "Remarks is required.";
                return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
            }

            // UPDATE or INSERT
            var result = await _context.AssessmentResults
                .FirstOrDefaultAsync(r => r.EnrollmentId == enrollmentId);

            if (result == null)
            {
                result = new AssessmentResult
                {
                    EnrollmentId = enrollmentId
                };
                _context.AssessmentResults.Add(result);
            }

            result.IsPassed = isPassed;
            result.Remarks = remarks;
            result.RecordDate = DateOnly.FromDateTime(DateTime.Now);
            result.RecordTime = TimeOnly.FromDateTime(DateTime.Now);

            enrollment.Status = "Completed";

            await _context.SaveChangesAsync();

            // Notify trainee
            string message = isPassed
                    ? "You passed the course"
                    : "You did not pass the course";

            await _notification.CreateNotification(enrollment.UserId, message);

            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        
        

        

            // ================= CERTIFICATION LOGIC =================
            if (isPassed)
            {
                int userId = enrollment.UserId;
                int courseId = enrollment.Session.CourseId;

                var trackCourses = await _context.CertificationTrackCourses
                    .Where(t => t.CourseId == courseId && t.IsRequired)
                    .ToListAsync();

                foreach (var track in trackCourses)
                {
                    int trackId = track.CertificationTrackId;

                    // Required courses for this track
                    var requiredCourses = await _context.CertificationTrackCourses
                        .Where(t => t.CertificationTrackId == trackId && t.IsRequired)
                        .Select(t => t.CourseId)
                        .ToListAsync();

                    // All passed courses by trainee
                    var passedCourses = await _context.AssessmentResults
                        .Include(a => a.Enrollment)
                            .ThenInclude(e => e.Session)
                        .Where(a => a.Enrollment.UserId == userId && a.IsPassed)
                        .Select(a => a.Enrollment.Session.CourseId)
                        .Distinct()
                        .ToListAsync();

                    // Only count required courses
                    var matchedPassed = passedCourses
                        .Where(pc => requiredCourses.Contains(pc))
                        .Distinct()
                        .ToList();

                    bool completed = requiredCourses.All(rc => matchedPassed.Contains(rc));

                    int percent = requiredCourses.Count == 0
                        ? 0
                        : (matchedPassed.Count * 100) / requiredCourses.Count;

                    // Get or create progress
                    var progress = await _context.TraineeCertificationProgresses
                        .FirstOrDefaultAsync(p =>
                            p.UserId == userId &&
                            p.CertificationTrackId == trackId);

                    if (progress == null)
                    {
                        progress = new TraineeCertificationProgress
                        {
                            UserId = userId,
                            CertificationTrackId = trackId,
                            Status = completed ? "Eligible" : "In Progress",
                            ProgressPercent = percent,
                            EligibleDate = completed
                                ? DateOnly.FromDateTime(DateTime.Now)
                                : DateOnly.MinValue
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

                    // ================= CREATE CERTIFICATE =================
                    if (completed)
                    {
                        bool exists = await _context.Certificates
                            .AnyAsync(c =>
                                c.UserId == userId &&
                                c.CertificationTrackId == trackId);

                        if (!exists)
                        {
                            var certificate = new Certificate
                            {
                                UserId = userId,
                                CertificationTrackId = trackId,
                                CertificateStatus = "Issued",
                                CertificateReferenceNumber = Guid.NewGuid().ToString(),
                                IssuedDate = DateOnly.FromDateTime(DateTime.Now)
                            };

                            _context.Certificates.Add(certificate);

                            await _notification.CreateNotification(userId, "Your certificate is ready");
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        }
    }
}