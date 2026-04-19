using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class EnrollmentManagementController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

        public EnrollmentManagementController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
                return NotFound();

            if (enrollment.Status != "Enrolled")
            {
                TempData["Error"] = "Only enrolled records can be confirmed.";
                return RedirectToAction(nameof(Index));
            }

            if (enrollment.OutstandingBalance > 0)
            {
                TempData["Error"] = "Cannot confirm until payment is completed.";
                return RedirectToAction(nameof(Index));
            }

            enrollment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            await _notification.CreateNotification(
                enrollment.UserId,
                $"Your enrollment for {enrollment.Session.Course.Title} has been confirmed."
            );

            TempData["Success"] = "Enrollment confirmed successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttending(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
                return NotFound();

            if (enrollment.Status != "Confirmed")
            {
                TempData["Error"] = "Only confirmed enrollments can be marked as attending.";
                return RedirectToAction(nameof(Index));
            }

            enrollment.Status = "Attending";
            await _context.SaveChangesAsync();

            await _notification.CreateNotification(
                enrollment.UserId,
                $"You are now marked as attending for {enrollment.Session.Course.Title}."
            );

            TempData["Success"] = "Enrollment marked as attending.";
            return RedirectToAction(nameof(Index));
        }
    }
}