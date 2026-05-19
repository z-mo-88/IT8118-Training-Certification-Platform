using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing equipment
    public class EquipmentController : BaseController
    {
        // Database context used for database operations
        private readonly AppDbContext _context;

        // Constructor injection
        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        // ================= DISPLAY EQUIPMENT =================
        // Displays all equipment records
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Return equipment list to page
            return View(await _context.Equipment.ToListAsync());
        }

        // ================= CREATE GET =================
        // Opens create equipment page
        [HttpGet]
        public IActionResult Create()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        // ================= CREATE POST =================
        // Saves new equipment into database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate equipment data
            ValidateEquipment(equipment);

            // If validation fails
            if (!ModelState.IsValid)
                return View(equipment);

            // Add equipment to database
            _context.Equipment.Add(equipment);

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect to equipment list
            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================
        // Opens edit page for selected equipment
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find equipment by ID
            var equipment = await _context.Equipment.FindAsync(id);

            // Return 404 if not found
            if (equipment == null)
                return NotFound();

            return View(equipment);
        }

        // ================= EDIT POST =================
        // Updates existing equipment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Equipment equipment)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate equipment
            ValidateEquipment(equipment);

            // If validation fails
            if (!ModelState.IsValid)
                return View(equipment);

            // Find existing equipment in database
            var existingEquipment = await _context.Equipment.FindAsync(equipment.EquipmentId);

            // Return 404 if not found
            if (existingEquipment == null)
                return NotFound();

            // Update equipment fields
            existingEquipment.EquipmentName = equipment.EquipmentName;
            existingEquipment.Description = equipment.Description;

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect back to equipment list
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE GET =================
        // Opens delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find equipment by ID
            var item = await _context.Equipment.FindAsync(id);

            // Return 404 if not found
            if (item == null)
                return NotFound();

            return View(item);
        }

        // ================= DELETE POST =================
        // Deletes equipment after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find equipment by ID
            var item = await _context.Equipment.FindAsync(id);

            // Return 404 if not found
            if (item == null)
                return NotFound();

            // ================= VALIDATION BEFORE DELETE =================
            // Check if equipment is already used in room equipment
            bool usedInRoomEquipment = await _context.RoomEquipments
                .AnyAsync(r => r.EquipmentId == id);

            // Check if equipment is used in course requirements
            bool usedInCourseRequirements = await _context.CourseEquipmentRequirements
                .AnyAsync(c => c.EquipmentId == id);

            // Prevent deleting equipment if already linked
            if (usedInRoomEquipment || usedInCourseRequirements)
            {
                TempData["ErrorMessage"] =
                    "This equipment cannot be deleted because it is already used in room equipment or course requirements.";

                return RedirectToAction(nameof(Index));
            }

            // Remove equipment from database
            _context.Equipment.Remove(item);

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect back to equipment list
            return RedirectToAction(nameof(Index));
        }

        // ================= CUSTOM VALIDATION =================
        // Validates equipment fields
        private void ValidateEquipment(Equipment equipment)
        {
            // Equipment name cannot be empty
            if (string.IsNullOrWhiteSpace(equipment.EquipmentName))
                ModelState.AddModelError("EquipmentName", "Equipment name is required");

            // Description cannot be empty
            if (string.IsNullOrWhiteSpace(equipment.Description))
                ModelState.AddModelError("Description", "Description is required");
        }
    }
}