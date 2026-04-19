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

        // ================= VIEW MY ENROLLMENTS =================
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

        // ================= ENROLL =================
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
            {
                TempData["Error"] = "Session not found";
                return RedirectToAction("Index", "Courses");
            }

            // Prevent enrolling in same COURSE 
            bool alreadyEnrolledInCourse = await _context.Enrollments
                .Include(e => e.Session)
                .AnyAsync(e =>
                    e.UserId == userId &&
                    e.Status != "Dropped" &&
                    e.Session.CourseId == session.CourseId);

            if (alreadyEnrolledInCourse)
            {
                TempData["Error"] = "You are already enrolled in this course";
                return RedirectToAction("Index", "Courses");
            }

            //No seats
            if (session.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats";
                return RedirectToAction("Index", "Courses");
            }

            //Prerequisite check
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
                    TempData["Error"] = "You must complete prerequisite first";
                    return RedirectToAction("Index", "Courses");
                }
            }

            // Create enrollment
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

            TempData["Success"] = "Enrollment successful!";

            return RedirectToAction("Index", "Courses");
        }
        // ================= CONFIRM =================
        public async Task<IActionResult> Confirm(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            if (enrollment.OutstandingBalance > 0)
            {
                TempData["Error"] = "Cannot confirm until payment is completed";
                return RedirectToAction("Index", "Courses");
            }

            enrollment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Enrollment confirmed!";

            return RedirectToAction("Index", "Courses");
        }

        // ================= DROP =================
        public async Task<IActionResult> Drop(int id)
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
                return NotFound();

            enrollment.Status = "Dropped";

            
            var session = await _context.CourseSessions.FindAsync(enrollment.SessionId);
            if (session != null)
                session.AvailableSeats++;

            await _context.SaveChangesAsync();
            await _notification.CreateNotification(
    session.UserId,
    "A trainee enrolled in your session"
);

            TempData["Success"] = "Course dropped successfully";

          
            return RedirectToAction(nameof(Index));
        }
    }
}