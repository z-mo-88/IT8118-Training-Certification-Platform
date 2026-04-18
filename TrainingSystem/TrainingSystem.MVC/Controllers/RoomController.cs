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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateRoom(room);

            if (!ModelState.IsValid)
                return View(room);

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateRoom(room);

            if (!ModelState.IsValid)
                return View(room);

            var existingRoom = await _context.Rooms.FindAsync(room.RoomId);
            if (existingRoom == null)
                return NotFound();

            existingRoom.RoomName = room.RoomName;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Location = room.Location;

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

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound();

            bool hasSessions = await _context.CourseSessions.AnyAsync(s => s.RoomId == id);
            bool hasEquipment = await _context.RoomEquipments.AnyAsync(re => re.RoomId == id);

            if (hasSessions || hasEquipment)
            {
                TempData["ErrorMessage"] = "This room cannot be deleted because it is already used in sessions or room equipment.";
                return RedirectToAction(nameof(Index));
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void ValidateRoom(Room room)
        {
            if (string.IsNullOrWhiteSpace(room.RoomName))
                ModelState.AddModelError("RoomName", "Room name is required");

            if (room.Capacity <= 0)
                ModelState.AddModelError("Capacity", "Capacity must be greater than 0");

            if (string.IsNullOrWhiteSpace(room.Location))
                ModelState.AddModelError("Location", "Location is required");
        }
    }
}