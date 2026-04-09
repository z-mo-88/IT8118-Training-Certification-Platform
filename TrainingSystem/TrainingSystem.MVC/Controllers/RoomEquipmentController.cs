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
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomName");
            ViewBag.Equipments = new SelectList(_context.Equipment, "EquipmentId", "EquipmentName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoomEquipment model)
        {
            _context.RoomEquipments.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.RoomEquipments.FindAsync(id);

            _context.RoomEquipments.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}