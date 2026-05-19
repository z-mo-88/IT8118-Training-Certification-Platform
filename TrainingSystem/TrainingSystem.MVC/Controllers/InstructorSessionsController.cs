using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for instructor session management
    // Instructor can:
    // 1. View assigned sessions
    // 2. View session details
    // 3. View students
    // 4. Mark students as attending
    // 5. Record pass/fail results
    public class InstructorSessionsController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Notification service
        private readonly NotificationService _notification;

        // Constructor injection
        public InstructorSessionsController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // ================= MY SESSIONS =================
        // Displays all sessions assigned to the instructor
        public async Task<IActionResult> Index()
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int instructorId = UserId.Value;

            // Load instructor sessions with related Course and Room data
            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)

                // Only sessions assigned to this instructor
                .Where(s => s.UserId == instructorId)

                // Sort by date and time
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)

                .ToListAsync();

            return View(sessions);
        }

        // ================= SESSION DETAILS =================
        // Displays details for one session
        public async Task<IActionResult> Details(int id)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int instructorId = UserId.Value;

            // Load session with Course and Room data
            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)

                // Ensure instructor owns this session
                .FirstOrDefaultAsync(s =>
                    s.SessionId == id &&
                    s.UserId == instructorId);

            // Return 404 if session not found
            if (session == null)
                return NotFound();

            return View(session);
        }

        // ================= SESSION STUDENTS =================
        // Displays students enrolled in a session
        public async Task<IActionResult> Students(int id)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int instructorId = UserId.Value;

            // Load session with enrolled students and assessment results
            var session = await _context.CourseSessions
                .Include(s => s.Course)

                // Include enrollments and related users
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.User)

                // Include assessment results
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.AssessmentResults)

                // Ensure instructor owns this session
                .FirstOrDefaultAsync(s =>
                    s.SessionId == id &&
                    s.UserId == instructorId);

            // Return 404 if not found
            if (session == null)
                return NotFound();

            return View(session);
        }

        // ================= MARK ATTENDING =================
        // Marks student as attending
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttending(int enrollmentId)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int instructorId = UserId.Value;

            // Find enrollment linked to instructor session
            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == enrollmentId &&
                    e.Session.UserId == instructorId);

            // Return 404 if not found
            if (enrollment == null)
                return NotFound();

            // Update enrollment status
            enrollment.Status = "Attending";

            // Save changes
            await _context.SaveChangesAsync();

            // Return back to students page
            return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
        }

        // ================= RECORD RESULT =================
        // Records pass/fail result for trainee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(int enrollmentId, bool isPassed, string? remarks)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int instructorId = UserId.Value;

            // Load enrollment with session, course, and assessment results
            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)

                .Include(e => e.AssessmentResults)

                // Ensure instructor owns this session
                .FirstOrDefaultAsync(e =>
                    e.EnrollmentId == enrollmentId &&
                    e.Session.UserId == instructorId);

            // Return 404 if not found
            if (enrollment == null)
                return NotFound();

            // ================= VALIDATE REMARKS =================
            // Remarks are required before recording result
            if (string.IsNullOrWhiteSpace(remarks))
            {
                ModelState.AddModelError("Remarks_" + enrollmentId, "Please enter remarks");

                TempData["Error_" + enrollmentId] = "Please enter remarks";

                return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
            }

            // ================= SAVE RESULT =================
            // Check if assessment result already exists
            var existingResult = enrollment.AssessmentResults.FirstOrDefault();

            // Create new result if none exists
            if (existingResult == null)
            {
                var result = new AssessmentResult
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    IsPassed = isPassed,
                    Remarks = remarks.Trim(),

                    // Save current date and time
                    RecordDate = DateOnly.FromDateTime(DateTime.Now),
                    RecordTime = TimeOnly.FromDateTime(DateTime.Now)
                };

                _context.AssessmentResults.Add(result);
            }
            else
            {
                // Update existing result
                existingResult.IsPassed = isPassed;
                existingResult.Remarks = remarks.Trim();

                existingResult.RecordDate = DateOnly.FromDateTime(DateTime.Now);
                existingResult.RecordTime = TimeOnly.FromDateTime(DateTime.Now);
            }

            // If trainee passed, mark enrollment as completed
            if (isPassed)
            {
                enrollment.Status = "Completed";
            }

            // Save result changes
            await _context.SaveChangesAsync();

            // ================= SEND RESULT NOTIFICATION =================
            var courseName = enrollment.Session.Course.Title;

            string message = isPassed
                ? $"You passed the course: {courseName}"
                : $"You did not pass the course: {courseName}";

            // Send notification to trainee
            await _notification.CreateNotification(enrollment.UserId, message);

            // ================= CERTIFICATION LOGIC =================
            // Only continue if trainee passed
            if (isPassed)
            {
                int userId = enrollment.UserId;
                int courseId = enrollment.Session.CourseId;

                // Get certification tracks related to this course
                var trackIds = await _context.CertificationTrackCourses
                    .Where(t => t.CourseId == courseId)
                    .Select(t => t.CertificationTrackId)
                    .Distinct()
                    .ToListAsync();

                // Loop through all related tracks
                foreach (var trackId in trackIds)
                {
                    // ================= REQUIRED COURSES =================
                    // Get required courses for certification track
                    var requiredCourses = await _context.CertificationTrackCourses
                        .Where(t => t.CertificationTrackId == trackId && t.IsRequired)
                        .Select(t => t.CourseId)
                        .ToListAsync();

                    // If no required courses exist, use all courses
                    if (!requiredCourses.Any())
                    {
                        requiredCourses = await _context.CertificationTrackCourses
                            .Where(t => t.CertificationTrackId == trackId)
                            .Select(t => t.CourseId)
                            .ToListAsync();
                    }

                    // ================= PASSED COURSES =================
                    // Get all passed courses for trainee
                    var passedCourses = await _context.AssessmentResults
                        .Include(a => a.Enrollment)
                            .ThenInclude(e => e.Session)

                        .Where(a => a.Enrollment.UserId == userId && a.IsPassed)

                        .Select(a => a.Enrollment.Session.CourseId)

                        .Distinct()
                        .ToListAsync();

                    // Match passed courses with required track courses
                    var matchedPassed = passedCourses
                        .Where(pc => requiredCourses.Contains(pc))
                        .Distinct()
                        .ToList();

                    // ================= CERTIFICATION STATUS =================
                    bool hasStarted = matchedPassed.Any();

                    bool completed = requiredCourses.Any() &&
                                     requiredCourses.All(rc => matchedPassed.Contains(rc));

                    // Calculate completion percentage
                    int percent = requiredCourses.Count == 0
                        ? 0
                        : (matchedPassed.Count * 100) / requiredCourses.Count;

                    // Determine certification progress status
                    string status;

                    if (!hasStarted)
                        status = "Not Started";
                    else if (!completed)
                        status = "In Progress";
                    else
                        status = "Eligible";

                    // ================= UPDATE PROGRESS TABLE =================
                    // Find trainee certification progress
                    var progress = await _context.TraineeCertificationProgresses
                        .FirstOrDefaultAsync(p =>
                            p.UserId == userId &&
                            p.CertificationTrackId == trackId);

                    // Create progress record if it does not exist
                    if (progress == null)
                    {
                        progress = new TraineeCertificationProgress
                        {
                            UserId = userId,
                            CertificationTrackId = trackId
                        };

                        _context.TraineeCertificationProgresses.Add(progress);
                    }

                    // Update progress information
                    progress.Status = status;
                    progress.ProgressPercent = percent;

                    // Save eligible date when certification completed
                    if (completed)
                        progress.EligibleDate = DateOnly.FromDateTime(DateTime.Now);

                    // ================= GENERATE CERTIFICATE =================
                    if (completed)
                    {
                        // Prevent duplicate certificates
                        bool exists = await _context.Certificates
                            .AnyAsync(c =>
                                c.UserId == userId &&
                                c.CertificationTrackId == trackId);

                        // Create certificate if it does not already exist
                        if (!exists)
                        {
                            _context.Certificates.Add(new Certificate
                            {
                                UserId = userId,
                                CertificationTrackId = trackId,

                                CertificateStatus = "Issued",

                                // Generate unique certificate reference number
                                CertificateReferenceNumber = Guid.NewGuid().ToString(),

                                IssuedDate = DateOnly.FromDateTime(DateTime.Now)
                            });

                            // Get certification track name
                            var trackName = await _context.CertificationTracks
                                .Where(t => t.CertificationTrackId == trackId)
                                .Select(t => t.TrackName)
                                .FirstOrDefaultAsync();

                            // Notify trainee that certificate is ready
                            await _notification.CreateNotification(
                                userId,
                                $"Your certificate for {trackName} is ready!"
                            );
                        }
                    }
                }

                // Save all certification updates
                await _context.SaveChangesAsync();
            }

            // Return back to students page
            return RedirectToAction(nameof(Students), new { id = enrollment.SessionId });
        }
    }
}