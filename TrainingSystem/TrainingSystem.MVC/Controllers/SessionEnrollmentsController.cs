using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for showing students enrolled in a specific session
    public class SessionEnrollmentsController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Notification service
        private readonly NotificationService _notification;

        // Constructor injection
        public SessionEnrollmentsController(AppDbContext context)
        {
            _context = context;

            // Create notification service using the same database context
            _notification = new NotificationService(_context);
        }

        //  VIEW STUDENTS 
        // Displays students enrolled in a selected session
        public async Task<IActionResult> Index(int id)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Load enrollments for this session
            var enrollments = await _context.Enrollments

                // Include trainee/user information
                .Include(e => e.User)

                // Include assessment results
                .Include(e => e.AssessmentResults)

                // Include session information
                .Include(e => e.Session)

                    // Include course information for the session
                    .ThenInclude(s => s.Course)

                // Show only students in this session and exclude dropped enrollments
                .Where(e => e.SessionId == id && e.Status != "Dropped")

                .ToListAsync();

            // Send session ID to the view
            ViewBag.SessionId = id;

            return View(enrollments);
        }

        // ================= MARK ATTENDING =================
        // Changes enrollment status to Attending
        public async Task<IActionResult> MarkAttending(int id)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Find enrollment by ID
            var enrollment = await _context.Enrollments.FindAsync(id);

            // Return 404 if enrollment does not exist
            if (enrollment == null)
                return NotFound();

            // Update enrollment status
            enrollment.Status = "Attending";

            // Save changes to database
            await _context.SaveChangesAsync();

            // Return back to the same session students page
            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        }
    }
}