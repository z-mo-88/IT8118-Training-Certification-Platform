using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class EnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        //  VIEW 
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var enrollments = await _context.Enrollments
                .Include(e => e.Session)
                .ThenInclude(s => s.Course)
                .ToListAsync();

            return View(enrollments.Where(e => e.UserId == userId));
        }

        //  ENROLL 
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

            // Duplicate check
            if (await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.SessionId == sessionId))
            {
                TempData["Error"] = "Already enrolled";
                return RedirectToAction("Index", "CourseSession");
            }

            // Capacity check
            if (session.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats";
                return RedirectToAction("Index", "CourseSession");
            }

            // Prerequisite check
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
                    return RedirectToAction("Index", "CourseSession");
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

            await new NotificationController(_context)
                .CreateNotification(userId, "Enrollment successful");

            return RedirectToAction(nameof(Index));
        }

        // CONFIRM 
        public async Task<IActionResult> Confirm(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            enrollment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            await new NotificationController(_context)
                .CreateNotification(enrollment.UserId, "Enrollment confirmed");

            return RedirectToAction("Index", "CourseSession");
        }

        // DROP 
        public async Task<IActionResult> Drop(int id)
        {
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            enrollment.Status = "Dropped";

            var session = await _context.CourseSessions.FindAsync(enrollment.SessionId);
            if (session != null)
                session.AvailableSeats++; 

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}