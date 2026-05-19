using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for coordinator enrollment management
    // The coordinator can view enrollments, confirm them, and mark trainees as attending
    public class EnrollmentManagementController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Notification service used to send messages to trainees
        private readonly NotificationService _notification;

        // Constructor injection
        public EnrollmentManagementController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // Displays all enrollments for the coordinator
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load enrollments with trainee, session, and course information
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(enrollments);
        }

        // Confirms an enrollment after payment is completed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find enrollment with session and course details
            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            // Return 404 if enrollment does not exist
            if (enrollment == null)
                return NotFound();

            // Only enrollments with status "Enrolled" can be confirmed
            if (enrollment.Status != "Enrolled")
            {
                TempData["Error"] = "Only enrolled records can be confirmed.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent confirmation if trainee still has unpaid balance
            if (enrollment.OutstandingBalance > 0)
            {
                TempData["Error"] = "Cannot confirm until payment is completed.";
                return RedirectToAction(nameof(Index));
            }

            // Update enrollment status
            enrollment.Status = "Confirmed";

            // Save changes
            await _context.SaveChangesAsync();

            // Notify trainee that enrollment has been confirmed
            await _notification.CreateNotification(
                enrollment.UserId,
                $"Your enrollment for {enrollment.Session.Course.Title} has been confirmed."
            );

            TempData["Success"] = "Enrollment confirmed successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Marks confirmed enrollment as attending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttending(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find enrollment with session and course details
            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            // Return 404 if enrollment does not exist
            if (enrollment == null)
                return NotFound();

            // Only confirmed enrollments can be marked as attending
            if (enrollment.Status != "Confirmed")
            {
                TempData["Error"] = "Only confirmed enrollments can be marked as attending.";
                return RedirectToAction(nameof(Index));
            }

            // Update status to Attending
            enrollment.Status = "Attending";

            // Save changes
            await _context.SaveChangesAsync();

            // Notify trainee
            await _notification.CreateNotification(
                enrollment.UserId,
                $"You are now marked as attending for {enrollment.Session.Course.Title}."
            );

            TempData["Success"] = "Enrollment marked as attending.";

            return RedirectToAction(nameof(Index));
        }
    }
}