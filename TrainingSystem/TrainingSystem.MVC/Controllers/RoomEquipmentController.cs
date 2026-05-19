using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing room equipment
    public class RoomEquipmentController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor injection
        public RoomEquipmentController(AppDbContext context)
        {
            _context = context;
        }

        // Displays all room-equipment relationships
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load room equipment with related Room and Equipment data
            var data = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .ToListAsync();

            return View(data);
        }

        // Opens create page
        [HttpGet]
        public IActionResult Create()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load dropdown lists
            LoadDropdowns();

            return View();
        }

        // Saves new room-equipment relationship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomEquipment model)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Remove validation for navigation properties
            ModelState.Remove("Room");
            ModelState.Remove("Equipment");

            // Validate quantity
            if (model.Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0");
            }

            // Prevent duplicate room-equipment assignments
            bool duplicateExists = await _context.RoomEquipments.AnyAsync(r =>
                r.RoomId == model.RoomId &&
                r.EquipmentId == model.EquipmentId);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "This equipment is already assigned to this room.");
            }

            // Save if validation passes
            if (ModelState.IsValid)
            {
                _context.RoomEquipments.Add(model);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            LoadDropdowns();

            return View(model);
        }

        // Opens edit page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find selected room equipment
            var item = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RoomEquipmentId == id);

            if (item == null) return NotFound();

            LoadDropdowns();

            return View(item);
        }

        // Updates room-equipment relationship
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomEquipment model)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Remove validation for navigation properties
            ModelState.Remove("Room");
            ModelState.Remove("Equipment");

            // Validate quantity
            if (model.Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0");
            }

            // Prevent duplicate assignments
            bool duplicateExists = await _context.RoomEquipments.AnyAsync(r =>
                r.RoomEquipmentId != model.RoomEquipmentId &&
                r.RoomId == model.RoomId &&
                r.EquipmentId == model.EquipmentId);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "This equipment is already assigned to this room.");
            }

            // Save changes if validation passes
            if (ModelState.IsValid)
            {
                var existing = await _context.RoomEquipments.FindAsync(model.RoomEquipmentId);

                if (existing == null) return NotFound();

                // Update fields
                existing.RoomId = model.RoomId;
                existing.EquipmentId = model.EquipmentId;
                existing.Quantity = model.Quantity;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            LoadDropdowns();

            return View(model);
        }

        // Opens delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find selected room equipment
            var item = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RoomEquipmentId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // Deletes room-equipment relationship
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find record by ID
            var item = await _context.RoomEquipments.FindAsync(id);

            if (item == null)
                return NotFound();

            // Remove from database
            _context.RoomEquipments.Remove(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Loads dropdown lists for Rooms and Equipment
        private void LoadDropdowns()
        {
            ViewBag.Rooms = new SelectList(_context.Rooms.ToList(), "RoomId", "RoomName");

            ViewBag.Equipments = new SelectList(_context.Equipment.ToList(), "EquipmentId", "EquipmentName");
        }
    }
}