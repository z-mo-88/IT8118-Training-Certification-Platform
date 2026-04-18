using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class RoomEquipmentController : BaseController
    {
        private readonly AppDbContext _context;

        public RoomEquipmentController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var data = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .ToListAsync();

            return View(data);
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
        public async Task<IActionResult> Create(RoomEquipment model)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ModelState.Remove("Room");
            ModelState.Remove("Equipment");

            if (model.Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0");
            }

            bool duplicateExists = await _context.RoomEquipments.AnyAsync(r =>
                r.RoomId == model.RoomId &&
                r.EquipmentId == model.EquipmentId);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "This equipment is already assigned to this room.");
            }

            if (ModelState.IsValid)
            {
                _context.RoomEquipments.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var item = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RoomEquipmentId == id);

            if (item == null) return NotFound();

            LoadDropdowns();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomEquipment model)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ModelState.Remove("Room");
            ModelState.Remove("Equipment");

            if (model.Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than 0");
            }

            bool duplicateExists = await _context.RoomEquipments.AnyAsync(r =>
                r.RoomEquipmentId != model.RoomEquipmentId &&
                r.RoomId == model.RoomId &&
                r.EquipmentId == model.EquipmentId);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "This equipment is already assigned to this room.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.RoomEquipments.FindAsync(model.RoomEquipmentId);
                if (existing == null) return NotFound();

                existing.RoomId = model.RoomId;
                existing.EquipmentId = model.EquipmentId;
                existing.Quantity = model.Quantity;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var item = await _context.RoomEquipments
                .Include(r => r.Room)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.RoomEquipmentId == id);

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

            var item = await _context.RoomEquipments.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.RoomEquipments.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns()
        {
            ViewBag.Rooms = new SelectList(_context.Rooms.ToList(), "RoomId", "RoomName");
            ViewBag.Equipments = new SelectList(_context.Equipment.ToList(), "EquipmentId", "EquipmentName");
        }
    }
}