using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for trainee enrollments
    // Handles:
    // 1. Viewing enrollments
    // 2. Enrolling into sessions
    // 3. Confirming enrollments
    // 4. Dropping courses
    public class EnrollmentsController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Notification service
        private readonly NotificationService _notification;

        // Constructor injection
        public EnrollmentsController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // ================= VIEW MY ENROLLMENTS =================
        // Displays all enrollments for logged-in trainee
        public async Task<IActionResult> Index()
        {
            // Allow only Trainee role
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            // Get logged-in trainee ID
            int userId = UserId.Value;

            // Load enrollments with related Session and Course data
            var enrollments = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)

                // Only enrollments for current trainee
                .Where(e => e.UserId == userId)

                // Order by newest enrollment first
                .OrderByDescending(e => e.EnrollmentDate)

                .ToListAsync();

            return View(enrollments);
        }

        // ================= ENROLL =================
        // Allows trainee to enroll into a session
        [HttpPost]
        public async Task<IActionResult> Enroll(int sessionId)
        {
            // Allow only Trainee role
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            // Get logged-in trainee ID
            int userId = UserId.Value;

            // Load selected session with Course data
            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            // Return error if session does not exist
            if (session == null)
            {
                TempData["Error"] = "Session not found";

                return RedirectToAction("Index", "Courses");
            }

            // ================= DUPLICATE ENROLLMENT CHECK =================
            // Prevent trainee from enrolling into the same course more than once
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

            // ================= AVAILABLE SEATS CHECK =================
            // Prevent enrollment if no seats are available
            if (session.AvailableSeats <= 0)
            {
                TempData["Error"] = "No available seats";

                return RedirectToAction("Index", "Courses");
            }

            // ================= PREREQUISITE VALIDATION =================
            // Check if course has prerequisite
            if (session.Course.PrerequisiteCourseId != null)
            {
                // Ensure trainee passed prerequisite course
                bool passed = await _context.AssessmentResults
                    .Include(a => a.Enrollment)
                        .ThenInclude(e => e.Session)

                    .AnyAsync(a =>
                        a.Enrollment.UserId == userId &&
                        a.IsPassed &&
                        a.Enrollment.Session.CourseId == session.Course.PrerequisiteCourseId);

                // Prevent enrollment if prerequisite not completed
                if (!passed)
                {
                    TempData["Error"] = "You must complete prerequisite first";

                    return RedirectToAction("Index", "Courses");
                }
            }

            // ================= CREATE ENROLLMENT =================
            // Create new enrollment record
            var enrollment = new Enrollment
            {
                UserId = userId,
                SessionId = sessionId,

                // Default enrollment status
                Status = "Enrolled",

                // Save current enrollment date
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),

                // Set outstanding balance equal to course fee
                OutstandingBalance = session.Course.EnrollmentFee,

                // Default overdue status
                IsOverdue = false
            };

            // Reduce available seats after successful enrollment
            session.AvailableSeats--;

            // Add enrollment to database
            _context.Enrollments.Add(enrollment);

            // Save changes
            await _context.SaveChangesAsync();

            // Load trainee user data
            var user = await _context.Users.FindAsync(userId);

            // ================= SEND NOTIFICATION =================
            // Notify instructor about new enrollment
            await _notification.CreateNotification(
                session.UserId,
                $"{user.Name} enrolled in your session"
            );

            TempData["Success"] = "Enrollment successful!";

            return RedirectToAction("Index", "Courses");
        }

        // ================= CONFIRM ENROLLMENT =================
        // Allows coordinator to confirm enrollment after payment
        public async Task<IActionResult> Confirm(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find enrollment by ID
            var enrollment = await _context.Enrollments.FindAsync(id);

            // Return 404 if enrollment not found
            if (enrollment == null) return NotFound();

            // Prevent confirmation if payment not completed
            if (enrollment.OutstandingBalance > 0)
            {
                TempData["Error"] = "Cannot confirm until payment is completed";

                return RedirectToAction("Index", "Courses");
            }

            // Update enrollment status
            enrollment.Status = "Confirmed";

            // Save changes
            await _context.SaveChangesAsync();

            TempData["Success"] = "Enrollment confirmed!";

            return RedirectToAction("Index", "Courses");
        }

        // ================= DROP COURSE =================
        // Allows trainee to drop enrolled course
        public async Task<IActionResult> Drop(int id)
        {
            // Allow only Trainee role
            var auth = AuthorizeRole(1);
            if (auth != null) return auth;

            // Get logged-in trainee ID
            int userId = UserId.Value;

            // Load enrollment with related Session and Course
            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)

                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            // Return 404 if enrollment not found
            if (enrollment == null)
                return NotFound();

            // Update enrollment status to Dropped
            enrollment.Status = "Dropped";

            // ================= RETURN AVAILABLE SEAT =================
            // Increase available seats after trainee drops course
            var session = await _context.CourseSessions.FindAsync(enrollment.SessionId);

            if (session != null)
                session.AvailableSeats++;

            // Save changes
            await _context.SaveChangesAsync();

            // Load trainee data
            var user = await _context.Users.FindAsync(userId);

            // ================= SEND NOTIFICATION =================
            // Notify instructor that trainee dropped session
            await _notification.CreateNotification(
                session.UserId,
                $"{user.Name} has dropped your session"
            );

            TempData["Success"] = "Course dropped successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}