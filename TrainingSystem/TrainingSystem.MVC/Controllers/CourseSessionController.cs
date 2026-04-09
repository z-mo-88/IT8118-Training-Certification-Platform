using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CourseSessionController : BaseController
    {
        private readonly AppDbContext _context;

        public CourseSessionController(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Include(s => s.User)
                .ToListAsync();

            return View(sessions);
        }

        // CREATE 
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseSession session)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateSession(session);

            // Instructor availability
            bool isAvailable = await _context.InstructorAvailabilities
                .AnyAsync(a =>
                    a.UserId == session.UserId &&
                    a.DayOfWeek == session.SessionDate.DayOfWeek.ToString() &&
                    a.StartTime <= session.StartTime &&
                    a.EndTime >= session.EndTime);

            if (!isAvailable)
                ModelState.AddModelError("", "Instructor is not available");

            // Instructor conflict
            bool instructorConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.UserId == session.UserId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (instructorConflict)
                ModelState.AddModelError("", "Instructor already booked");

            // Room conflict
            bool roomConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.RoomId == session.RoomId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (roomConflict)
                ModelState.AddModelError("", "Room already booked");

            if (ModelState.IsValid)
            {
                session.AvailableSeats = session.SessionCapacity;
                session.Status = "Scheduled";

                _context.CourseSessions.Add(session);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(session);
        }

        // EDIT 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var session = await _context.CourseSessions.FindAsync(id);
            if (session == null) return NotFound();

            LoadDropdowns();
            return View(session);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CourseSession session)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ValidateSession(session); 

            if (ModelState.IsValid)
            {
                _context.CourseSessions.Update(session);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns();
            return View(session);
        }

        //  DELETE 
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return View(session);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var session = await _context.CourseSessions.FindAsync(id);

            if (session != null) 
            {
                _context.CourseSessions.Remove(session);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateSession(CourseSession session)
        {
            if (session.StartTime >= session.EndTime)
                ModelState.AddModelError("", "Start time must be before end time");

            if (session.SessionCapacity <= 0)
                ModelState.AddModelError("", "Capacity must be greater than 0");
        }

        private void LoadDropdowns()
        {
            ViewBag.Courses = new SelectList(_context.Courses, "CourseId", "Title");
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomName");
            ViewBag.Instructors = new SelectList(
                _context.Users.Where(u => u.RoleId == 2),
                "UserId",
                "Name"
            );
        }
    }
}