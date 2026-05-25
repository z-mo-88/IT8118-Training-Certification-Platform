using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class EquipmentController : BaseController
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var equipment = await _context.Equipment
                .Include(e => e.Provider)
                .ToListAsync();

            return View(equipment);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadProviders();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateEquipment(equipment);

            if (!ModelState.IsValid)
            {
                LoadProviders(equipment.ProviderId);
                return View(equipment);
            }

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var equipment = await _context.Equipment.FindAsync(id);

            if (equipment == null)
                return NotFound();

            LoadProviders(equipment.ProviderId);
            return View(equipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Equipment equipment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateEquipment(equipment);

            if (!ModelState.IsValid)
            {
                LoadProviders(equipment.ProviderId);
                return View(equipment);
            }

            var existingEquipment = await _context.Equipment.FindAsync(equipment.EquipmentId);

            if (existingEquipment == null)
                return NotFound();

            existingEquipment.EquipmentName = equipment.EquipmentName;
            existingEquipment.Description = equipment.Description;
            existingEquipment.ProviderId = equipment.ProviderId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var item = await _context.Equipment
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var item = await _context.Equipment.FindAsync(id);

            if (item == null)
                return NotFound();

            bool usedInRoomEquipment = await _context.RoomEquipments.AnyAsync(r => r.EquipmentId == id);
            bool usedInCourseRequirements = await _context.CourseEquipmentRequirements.AnyAsync(c => c.EquipmentId == id);

            if (usedInRoomEquipment || usedInCourseRequirements)
            {
                TempData["ErrorMessage"] = "This equipment cannot be deleted because it is already used in room equipment or course requirements.";
                return RedirectToAction(nameof(Index));
            }

            _context.Equipment.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadProviders(int? selectedProviderId = null)
        {
            ViewBag.Providers = new SelectList(
                _context.Providers
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.ProviderName)
                    .ToList(),
                "ProviderId",
                "ProviderName",
                selectedProviderId
            );
        }

        private void ValidateEquipment(Equipment equipment)
        {
            if (string.IsNullOrWhiteSpace(equipment.EquipmentName))
                ModelState.AddModelError("EquipmentName", "Equipment name is required.");

            if (string.IsNullOrWhiteSpace(equipment.Description))
                ModelState.AddModelError("Description", "Description is required.");

            if (equipment.ProviderId == null)
                ModelState.AddModelError("ProviderId", "Provider is required.");
        }
    }
}