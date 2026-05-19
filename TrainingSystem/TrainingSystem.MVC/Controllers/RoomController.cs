using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing training rooms
    public class RoomController : BaseController
    {
        // Database context used to access the Rooms table and related tables
        private readonly AppDbContext _context;

        // Constructor injection for AppDbContext
        public RoomController(AppDbContext context)
        {
            _context = context;
        }

        // ================= DISPLAY ROOMS =================
        // Displays all rooms to the coordinator
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Get all rooms from database and send them to the Index view
            return View(await _context.Rooms.ToListAsync());
        }

        // ================= CREATE GET =================
        // Opens the create room page
        [HttpGet]
        public IActionResult Create()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Return empty create form
            return View();
        }

        // ================= CREATE POST =================
        // Saves a new room into the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate room fields before saving
            ValidateRoom(room);

            // If validation fails, return the same form with errors
            if (!ModelState.IsValid)
                return View(room);

            // Add new room to database
            _context.Rooms.Add(room);

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect back to rooms list
            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================
        // Opens edit page for the selected room
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find room by ID
            var room = await _context.Rooms.FindAsync(id);

            // If room does not exist, return 404
            if (room == null)
                return NotFound();

            // Send room data to edit page
            return View(room);
        }

        // ================= EDIT POST =================
        // Updates existing room details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate updated room fields
            ValidateRoom(room);

            // If validation fails, return edit page with errors
            if (!ModelState.IsValid)
                return View(room);

            // Find the existing room from database
            var existingRoom = await _context.Rooms.FindAsync(room.RoomId);

            // If room is not found, return 404
            if (existingRoom == null)
                return NotFound();

            // Update room fields manually
            existingRoom.RoomName = room.RoomName;
            existingRoom.Capacity = room.Capacity;
            existingRoom.Location = room.Location;

            // Save updated data
            await _context.SaveChangesAsync();

            // Redirect back to rooms list
            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE GET =================
        // Opens delete confirmation page for a room
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find room by ID
            var room = await _context.Rooms.FindAsync(id);

            // If room is not found, return 404
            if (room == null)
                return NotFound();

            // Send room data to delete confirmation page
            return View(room);
        }

        // ================= DELETE POST =================
        // Deletes the room after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find room by ID
            var room = await _context.Rooms.FindAsync(id);

            // If room is not found, return 404
            if (room == null)
                return NotFound();

            // Check if this room is already assigned to any course sessions
            bool hasSessions = await _context.CourseSessions
                .AnyAsync(s => s.RoomId == id);

            // Check if this room already has equipment assigned to it
            bool hasEquipment = await _context.RoomEquipments
                .AnyAsync(re => re.RoomId == id);

            // Prevent deleting room if it is already used
            // This protects database relationships and avoids broken data
            if (hasSessions || hasEquipment)
            {
                TempData["ErrorMessage"] =
                    "This room cannot be deleted because it is already used in sessions or room equipment.";

                return RedirectToAction(nameof(Index));
            }

            // Remove room from database
            _context.Rooms.Remove(room);

            // Save changes
            await _context.SaveChangesAsync();

            // Redirect back to rooms list
            return RedirectToAction(nameof(Index));
        }

        // ================= CUSTOM VALIDATION =================
        // Validates room fields before create or edit
        private void ValidateRoom(Room room)
        {
            // Room name is required
            if (string.IsNullOrWhiteSpace(room.RoomName))
                ModelState.AddModelError("RoomName", "Room name is required");

            // Capacity must be greater than 0
            if (room.Capacity <= 0)
                ModelState.AddModelError("Capacity", "Capacity must be greater than 0");

            // Location is required
            if (string.IsNullOrWhiteSpace(room.Location))
                ModelState.AddModelError("Location", "Location is required");
        }
    }
}