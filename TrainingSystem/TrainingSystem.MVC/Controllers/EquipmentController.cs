using Microsoft.AspNetCore.Mvc;
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

            return View(await _context.Equipment.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (string.IsNullOrWhiteSpace(equipment.EquipmentName))
                ModelState.AddModelError("EquipmentName", "Equipment name is required");

            if (string.IsNullOrWhiteSpace(equipment.Description))
                ModelState.AddModelError("Description", "Description is required");

            if (!ModelState.IsValid)
                return View(equipment);

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var item = await _context.Equipment.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Equipment.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}