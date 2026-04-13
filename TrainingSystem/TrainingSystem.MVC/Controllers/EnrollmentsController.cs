using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class EnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

        public EnrollmentsController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var enrollments = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int sessionId)
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
                return NotFound();

            if (await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.SessionId == sessionId))
            {
                TempData["Error"] = "Already enrolled";
                return RedirectToAction("Index", "Courses");
            }

            if (session.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats";
                return RedirectToAction("Index", "Courses");
            }

            if (session.Course.PrerequisiteCourseId != null)
            {
                bool passed = await _context.AssessmentResults
                    .Include(a => a.Enrollment)
                        .ThenInclude(e => e.Session)
                    .AnyAsync(a =>
                        a.Enrollment.UserId == userId &&
                        a.IsPassed &&
                        a.Enrollment.Session.CourseId == session.Course.PrerequisiteCourseId);

                if (!passed)
                {
                    TempData["Error"] = "Complete prerequisite first";
                    return RedirectToAction("Index", "Courses");
                }
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                SessionId = sessionId,
                Status = "Enrolled",
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                OutstandingBalance = session.Course.EnrollmentFee,
                IsOverdue = session.Course.EnrollmentFee > 0
            };

            session.AvailableSeats--;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await _notification.Create(
                userId,
                $"You enrolled in {session.Course.Title}"
            );

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            enrollment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == enrollment.SessionId);

            if (session != null)
            {
                await _notification.Create(
                    enrollment.UserId,
                    $"Your enrollment in {session.Course.Title} has been confirmed"
                );
            }

            return RedirectToAction("Index", "Courses");
        }

        public async Task<IActionResult> Drop(int id)
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null) return NotFound();

            if (enrollment.Status == "Dropped")
                return RedirectToAction(nameof(Index));

            enrollment.Status = "Dropped";

            var session = await _context.CourseSessions.FindAsync(enrollment.SessionId);
            if (session != null)
                session.AvailableSeats++;

            await _context.SaveChangesAsync();

            await _notification.Create(
                enrollment.UserId,
                $"You dropped {enrollment.Session?.Course?.Title ?? "a course session"}"
            );

            return RedirectToAction(nameof(Index));
        }
    }
}