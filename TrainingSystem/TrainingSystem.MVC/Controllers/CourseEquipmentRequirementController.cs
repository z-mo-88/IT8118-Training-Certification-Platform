using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing equipment requirements for courses
    public class CourseEquipmentRequirementController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor injection
        public CourseEquipmentRequirementController(AppDbContext context)
        {
            _context = context;
        }

        // Displays all course equipment requirements
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load requirements with related Course and Equipment data
            var requirements = await _context.CourseEquipmentRequirements
                .Include(c => c.Course)
                .Include(c => c.Equipment)
                .ToListAsync();

            return View(requirements);
        }

        // Opens create requirement page
        [HttpGet]
        public IActionResult Create()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load course and equipment dropdowns
            LoadDropdowns();

            return View();
        }

        // Saves new course equipment requirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseEquipmentRequirement requirement)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate required fields
            ValidateRequirement(requirement);

            // Check if the same equipment is already required for the same course
            bool alreadyExists = await _context.CourseEquipmentRequirements
                .AnyAsync(r => r.CourseId == requirement.CourseId && r.EquipmentId == requirement.EquipmentId);

            // Prevent duplicate requirements
            if (alreadyExists)
            {
                ModelState.AddModelError("", "This equipment requirement already exists for the selected course.");
            }

            // If validation fails, reload dropdowns and return page
            if (!ModelState.IsValid)
            {
                LoadDropdowns();

                return View(requirement);
            }

            // Add requirement to database
            _context.CourseEquipmentRequirements.Add(requirement);

            // Save changes
            await _context.SaveChangesAsync();

            // Show success message
            TempData["Success"] = "Course equipment requirement added successfully.";

            // Redirect to list page
            return RedirectToAction(nameof(Index));
        }

        // Deletes a course equipment requirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find requirement by ID
            var requirement = await _context.CourseEquipmentRequirements.FindAsync(id);

            // Return 404 if not found
            if (requirement == null)
                return NotFound();

            // Remove requirement from database
            _context.CourseEquipmentRequirements.Remove(requirement);

            // Save changes
            await _context.SaveChangesAsync();

            // Show success message
            TempData["Success"] = "Course equipment requirement deleted successfully.";

            // Redirect to list page
            return RedirectToAction(nameof(Index));
        }

        // Loads dropdown lists for courses and equipment
        private void LoadDropdowns()
        {
            // Course dropdown
            ViewBag.Courses = new SelectList(
                _context.Courses.ToList(),
                "CourseId",
                "Title"
            );

            // Equipment dropdown
            ViewBag.Equipment = new SelectList(
                _context.Equipment.ToList(),
                "EquipmentId",
                "EquipmentName"
            );
        }

        // Validates course equipment requirement data
        private void ValidateRequirement(CourseEquipmentRequirement requirement)
        {
            // Course must be selected
            if (requirement.CourseId <= 0)
                ModelState.AddModelError("CourseId", "Please select a course.");

            // Equipment must be selected
            if (requirement.EquipmentId <= 0)
                ModelState.AddModelError("EquipmentId", "Please select equipment.");

            // Quantity must be greater than zero
            if (requirement.QuantityRequired <= 0)
                ModelState.AddModelError("QuantityRequired", "Quantity must be greater than 0.");
        }
    }
}