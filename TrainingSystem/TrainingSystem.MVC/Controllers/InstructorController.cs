using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for instructor management
    // It has two main parts:
    // 1. Coordinator can view and activate/deactivate instructors
    // 2. Instructor can view and update their own profile
    public class InstructorController : BaseController
    {
        // Database context used to access Users, InstructorProfile, Expertise, and Availability tables
        private readonly AppDbContext _context;

        // Constructor injection for database context
        public InstructorController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INSTRUCTOR LIST FOR COORDINATOR =================
        // Displays all instructors to the Training Coordinator
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Get users whose RoleId is 2, meaning Instructor
            var instructors = await _context.Users
                .Where(u => u.RoleId == 2)

                // Include instructor profile details such as Bio and Notes
                .Include(u => u.InstructorProfile)

                // Include instructor expertise records
                .Include(u => u.InstructorExpertises)

                    // Include the expertise name itself
                    .ThenInclude(ie => ie.Expertise)

                // Include instructor availability records
                .Include(u => u.InstructorAvailabilities)

                .ToListAsync();

            // Convert database entities into a display ViewModel for the page
            var model = instructors.Select(i => new InstructorDisplayViewModel
            {
                // Basic instructor user data
                UserId = i.UserId,
                Name = i.Name,
                Email = i.Email,
                PhoneNumber = i.PhoneNumber ?? "",
                IsActive = i.IsActive,

                // Instructor profile data
                Bio = i.InstructorProfile != null ? i.InstructorProfile.Bio : "",
                Notes = i.InstructorProfile != null ? i.InstructorProfile.Notes : "",

                // List of instructor expertise names
                ExpertiseNames = i.InstructorExpertises?
                    .Where(e => e.Expertise != null)
                    .Select(e => e.Expertise.ExpertiseName)
                    .Distinct()
                    .ToList() ?? new List<string>(),

                // List of instructor availability times
                AvailabilityText = i.InstructorAvailabilities?
                    .OrderBy(a => a.DayOfWeek)
                    .ThenBy(a => a.StartTime)
                    .Select(a => $"{a.DayOfWeek}: {a.StartTime} - {a.EndTime}")
                    .ToList() ?? new List<string>()
            }).ToList();

            // Send instructor display model to the view
            return View(model);
        }

        // ================= ACTIVATE / DEACTIVATE INSTRUCTOR =================
        // Allows coordinator to activate or deactivate an instructor account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find instructor by user ID and make sure the user role is Instructor
            var instructor = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 2);

            // If instructor does not exist, return 404
            if (instructor == null)
                return NotFound();

            // Toggle IsActive value
            // If active, deactivate. If inactive, activate.
            instructor.IsActive = !instructor.IsActive;

            // Save changes to database
            await _context.SaveChangesAsync();

            // Store success message depending on new status
            TempData["Success"] = instructor.IsActive
                ? "Instructor activated successfully."
                : "Instructor deactivated successfully.";

            // Redirect back to instructor list
            return RedirectToAction(nameof(Index));
        }

        // ================= INSTRUCTOR PROFILE GET =================
        // Opens instructor's own profile page
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID from BaseController
            int userId = UserId.Value;

            // Get user data for displaying name and email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            // Get instructor profile data for Bio and Notes
            var profile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // If profile does not exist, return 404
            if (profile == null)
                return NotFound();

            // Send name and email to the view using ViewBag
            ViewBag.Name = user?.Name;
            ViewBag.Email = user?.Email;

            // Send instructor profile model to the view
            return View(profile);
        }

        // ================= INSTRUCTOR PROFILE POST =================
        // Updates instructor's own profile
        [HttpPost]
        public async Task<IActionResult> Profile(InstructorProfile model)
        {
            // Allow only Instructor role
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Get logged-in instructor ID
            int userId = UserId.Value;

            // Find instructor profile
            var profile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // If profile does not exist, return 404
            if (profile == null)
                return NotFound();

            // Update profile fields
            profile.Bio = model.Bio;
            profile.Notes = model.Notes;

            // Save changes
            await _context.SaveChangesAsync();

            // Show success message
            TempData["Success"] = "Profile updated successfully";

            // Redirect back to profile page
            return RedirectToAction(nameof(Profile));
        }
    }
}