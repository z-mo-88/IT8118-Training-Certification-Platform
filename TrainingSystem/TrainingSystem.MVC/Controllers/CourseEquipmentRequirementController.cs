using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CourseEquipmentRequirementController : BaseController
    {
        private readonly AppDbContext _context;

        public CourseEquipmentRequirementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var requirements = await _context.CourseEquipmentRequirements
                .Include(c => c.Course)
                .Include(c => c.Equipment)
                .ToListAsync();

            return View(requirements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseEquipmentRequirement requirement)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateRequirement(requirement);

            bool alreadyExists = await _context.CourseEquipmentRequirements
                .AnyAsync(r => r.CourseId == requirement.CourseId && r.EquipmentId == requirement.EquipmentId);

            if (alreadyExists)
            {
                ModelState.AddModelError("", "This equipment requirement already exists for the selected course.");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(requirement);
            }

            _context.CourseEquipmentRequirements.Add(requirement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Course equipment requirement added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var requirement = await _context.CourseEquipmentRequirements.FindAsync(id);
            if (requirement == null)
                return NotFound();

            _context.CourseEquipmentRequirements.Remove(requirement);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Course equipment requirement deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Courses = new SelectList(
                _context.Courses.ToList(),
                "CourseId",
                "Title"
            );

            ViewBag.Equipment = new SelectList(
                _context.Equipment.ToList(),
                "EquipmentId",
                "EquipmentName"
            );
        }

        private void ValidateRequirement(CourseEquipmentRequirement requirement)
        {
            if (requirement.CourseId <= 0)
                ModelState.AddModelError("CourseId", "Please select a course.");

            if (requirement.EquipmentId <= 0)
                ModelState.AddModelError("EquipmentId", "Please select equipment.");

            if (requirement.QuantityRequired <= 0)
                ModelState.AddModelError("QuantityRequired", "Quantity must be greater than 0.");
        }
    }
}