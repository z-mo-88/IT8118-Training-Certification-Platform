using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class RoomController : BaseController
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View(await _context.Rooms.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Room room)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (string.IsNullOrWhiteSpace(room.RoomName))
                ModelState.AddModelError("RoomName", "Room name is required");

            if (room.Capacity <= 0)
                ModelState.AddModelError("Capacity", "Capacity must be greater than 0");

            if (string.IsNullOrWhiteSpace(room.Location))
                ModelState.AddModelError("Location", "Location is required");

            if (!ModelState.IsValid)
                return View(room);

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}