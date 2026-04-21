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

            var existingResult = enrollment.AssessmentResults.FirstOrDefault();

            if (existingResult == null)
            {
                var result = new AssessmentResult
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    IsPassed = isPassed,
                    Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
                    RecordDate = DateOnly.FromDateTime(DateTime.Now),
                    RecordTime = TimeOnly.FromDateTime(DateTime.Now)
                };

                _context.AssessmentResults.Add(result);
            }
            else
            {
                existingResult.IsPassed = isPassed;
                existingResult.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
                existingResult.RecordDate = DateOnly.FromDateTime(DateTime.Now);
                existingResult.RecordTime = TimeOnly.FromDateTime(DateTime.Now);
            }

            // COMPLETE
            if (isPassed)
            {
                enrollment.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            var courseName = enrollment.Session.Course.Title;

            if (isPassed)
            {
                await _notification.CreateNotification(
                    enrollment.UserId,
                    $"You passed {courseName}"
                );
            }
            else
            {
                await _notification.CreateNotification(
                    enrollment.UserId,
                    $"You did not pass {courseName}"
                );
            }

            return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
        }
    }
}