using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing courses
    public class CoursesController : BaseController
    {
        // Database context used to access database tables
        private readonly AppDbContext _context;

        // Constructor injection for database context
        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ================= DISPLAY COURSES =================
        // Displays all courses with related data
        public async Task<IActionResult> Index()
        {
            // Get logged-in user ID from session
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Load courses with related tables
            var courses = await _context.Courses

                // Include category information
                .Include(c => c.Category)

                // Include prerequisite course
                .Include(c => c.PrerequisiteCourse)

                // Include course sessions
                .Include(c => c.CourseSessions)

                    // Include instructor user
                    .ThenInclude(s => s.User)

                        // Include instructor profile
                        .ThenInclude(u => u.InstructorProfile)

                // Include enrollments inside sessions
                .Include(c => c.CourseSessions)
                    .ThenInclude(s => s.Enrollments)

                .ToListAsync();

            // Check if user is logged in
            if (userId != null)
            {
                // ================= ENROLLED SESSIONS =================
                // Get session IDs where trainee already enrolled
                var enrolledSessionIds = await _context.Enrollments
                    .Where(e => e.UserId == userId && e.Status == "Enrolled")
                    .Select(e => e.SessionId)
                    .ToListAsync();

                // Send enrolled sessions to view
                ViewBag.EnrolledSessionIds = enrolledSessionIds;

                // ================= PASSED COURSES =================
                // Get courses passed by trainee
                var passedCourseIds = await _context.AssessmentResults
                    .Include(a => a.Enrollment)
                        .ThenInclude(e => e.Session)

                    .Where(a => a.Enrollment.UserId == userId && a.IsPassed)

                    // Get passed course IDs
                    .Select(a => a.Enrollment.Session.CourseId)

                    .Distinct()
                    .ToListAsync();

                // Send passed courses to view
                ViewBag.PassedCourseIds = passedCourseIds;
            }

            // Return courses to page
            return View(courses);
        }

        // ================= CREATE GET =================
        // Opens create course page
        public IActionResult Create()
        {
            // Allow only coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load dropdown lists
            LoadDropdowns();

            return View();
        }

        // ================= CREATE POST =================
        // Saves new course into database
        [HttpPost]
        public async Task<IActionResult> Create(Course course)
        {
            // Allow only coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate course data
            ValidateCourse(course);

            // Ensure category is selected
            if (course.CategoryId == 0)
                ModelState.AddModelError("CategoryId", "Please select a category");

            // If validation fails
            if (!ModelState.IsValid)
            {
                // Reload dropdown lists
                LoadDropdowns();

                return View(course);
            }

            // Add course to database
            _context.Courses.Add(course);

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect to course list
            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================
        // Opens edit page for selected course
        public async Task<IActionResult> Edit(int id)
        {
            // Allow only coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find course by ID
            var course = await _context.Courses.FindAsync(id);

            // Return 404 if not found
            if (course == null) return NotFound();

            // Load dropdown lists
            LoadDropdowns();

            return View(course);
        }

        // ================= EDIT POST =================
        // Updates existing course
        [HttpPost]
        public async Task<IActionResult> Edit(Course course)
        {
            // Allow only coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate course
            ValidateCourse(course);

            // Ensure category selected
            if (course.CategoryId == 0)
                ModelState.AddModelError("CategoryId", "Please select a category");

            // If validation fails
            if (!ModelState.IsValid)
            {
                // Remove duplicate validation issue
                ModelState.Remove("CategoryId");

                // Reload dropdowns
                LoadDropdowns();

                return View(course);
            }

            // Update course in database
            _context.Courses.Update(course);

            // Save changes
            await _context.SaveChangesAsync();

            // Return to course list
            return RedirectToAction(nameof(Index));
        }

        // ================= VALIDATION =================
        // Custom validation method for course fields
        private void ValidateCourse(Course course)
        {
            // Duration must be positive
            if (course.DurationHours <= 0)
                ModelState.AddModelError("", "Duration must be greater than 0");

            // Capacity must be positive
            if (course.DefaultCapacity <= 0)
                ModelState.AddModelError("", "Capacity must be greater than 0");

            // Fee cannot be negative
            if (course.EnrollmentFee < 0)
                ModelState.AddModelError("", "Fee cannot be negative");
        }

        // ================= DROPDOWNS =================
        // Loads dropdown lists for categories and prerequisite courses
        private void LoadDropdowns()
        {
            // Category dropdown
            ViewBag.Categories = new SelectList(
                _context.SubjectCategories,
                "CategoryId",
                "CategoryName"
            );

            // Courses dropdown (used for prerequisites)
            ViewBag.Courses = new SelectList(
                _context.Courses,
                "CourseId",
                "Title"
            );
        }
    }
}