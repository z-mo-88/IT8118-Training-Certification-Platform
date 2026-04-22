using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorSessionsController : BaseController
    {
        private readonly AppDbContext _context;

        private readonly NotificationService _notification;


        public InstructorSessionsController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // ================= MY SESSIONS =================
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Where(s => s.UserId == instructorId)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(sessions);
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s =>
                    s.SessionId == id &&
                    s.UserId == instructorId);

            if (session == null)
                return NotFound();

            return View(session);
        }

        // ================= STUDENTS =================
        public async Task<IActionResult> Students(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.User)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.AssessmentResults)
                .FirstOrDefaultAsync(s =>
                    s.SessionId == id &&
                    s.UserId == instructorId);

            if (session == null)
                return NotFound();

            return View(session);
        }

        // ================= MARK ATTENDING =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttending(int enrollmentId)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == enrollmentId &&
                    e.Session.UserId == instructorId);

            if (enrollment == null)
                return NotFound();

            enrollment.Status = "Attending";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
        }

        // ================= RECORD RESULT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(int enrollmentId, bool isPassed, string? remarks)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course) 
                .Include(e => e.AssessmentResults)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == enrollmentId &&
                    e.Session.UserId == instructorId);

            if (enrollment == null)
                return NotFound();

            
            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["Error"] = "Please enter remarks";
                return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
            }

            // ===== SAVE RESULT =====
            var existingResult = enrollment.AssessmentResults.FirstOrDefault();

            if (existingResult == null)
            {
                var result = new AssessmentResult
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    IsPassed = isPassed,
                    Remarks = remarks.Trim(),
                    RecordDate = DateOnly.FromDateTime(DateTime.Now),
                    RecordTime = TimeOnly.FromDateTime(DateTime.Now)
                };

                _context.AssessmentResults.Add(result);
            }
            else
            {
                existingResult.IsPassed = isPassed;
                existingResult.Remarks = remarks.Trim();
                existingResult.RecordDate = DateOnly.FromDateTime(DateTime.Now);
                existingResult.RecordTime = TimeOnly.FromDateTime(DateTime.Now);
            }

            if (isPassed)
            {
                enrollment.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            // ===== NOTIFICATION =====
            var courseName = enrollment.Session.Course.Title;

            string message = isPassed
                ? $"You passed the course: {courseName}"
                : $"You did not pass the course: {courseName}";

            await _notification.CreateNotification(enrollment.UserId, message);

            // ===== CERTIFICATION LOGIC =====
            if (isPassed)
            {
                int userId = enrollment.UserId;
                int courseId = enrollment.Session.CourseId;

                // Only tracks related to this course
                var trackIds = await _context.CertificationTrackCourses
                    .Where(t => t.CourseId == courseId)
                    .Select(t => t.CertificationTrackId)
                    .Distinct()
                    .ToListAsync();

                foreach (var trackId in trackIds)
                {
                    // Get required courses
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

                    bool completed = requiredCourses.All(rc => passedCourses.Contains(rc));

                    int percent = requiredCourses.Count == 0
                        ? 0
                        : (passedCourses.Count(pc => requiredCourses.Contains(pc)) * 100) / requiredCourses.Count;

                    // ===== PROGRESS =====
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

                    // ===== CERTIFICATE =====
                    if (completed)
                    {
                        bool exists = await _context.Certificates
                            .AnyAsync(c =>
                                c.UserId == userId &&
                                c.CertificationTrackId == trackId);

                        if (!exists)
                        {
                            _context.Certificates.Add(new Certificate
                            {
                                UserId = userId,
                                CertificationTrackId = trackId,
                                CertificateStatus = "Issued",
                                CertificateReferenceNumber = Guid.NewGuid().ToString(),
                                IssuedDate = DateOnly.FromDateTime(DateTime.Now)
                            });

                            var trackName = await _context.CertificationTracks
                                .Where(t => t.CertificationTrackId == trackId)
                                .Select(t => t.TrackName)
                                .FirstOrDefaultAsync();

                            await _notification.CreateNotification(
                                userId,
                                $"Your certificate for {trackName} is ready!"
                            );
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
        }
    }
}