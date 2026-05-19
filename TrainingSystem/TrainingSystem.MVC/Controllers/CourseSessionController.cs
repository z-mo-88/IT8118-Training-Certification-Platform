using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for managing course sessions
    // A session represents the actual scheduled class
    public class CourseSessionController : BaseController
    {
        // Database context
        private readonly AppDbContext _context;

        // Notification service used to send notifications
        private readonly NotificationService _notification;

        // Constructor injection
        public CourseSessionController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // ================= DISPLAY SESSIONS =================
        // Displays all course sessions
        public async Task<IActionResult> Index()
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load sessions with related Course, Room, and Instructor data
            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Include(s => s.User)
                .ToListAsync();

            return View(sessions);
        }

        // ================= CREATE GET =================
        // Opens create session page
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

        // ================= CREATE POST =================
        // Creates a new course session
        [HttpPost]
        public async Task<IActionResult> Create(CourseSession session)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Remove navigation property validation
            ModelState.Remove("Course");
            ModelState.Remove("Room");
            ModelState.Remove("User");
            ModelState.Remove("Status");

            // Validate session fields
            ValidateSession(session);

            // ================= ROOM CAPACITY VALIDATION =================
            // Ensure session capacity does not exceed room capacity
            var room = await _context.Rooms.FindAsync(session.RoomId);

            if (room != null && session.SessionCapacity > room.Capacity)
            {
                ModelState.AddModelError("", "Session capacity exceeds room capacity");
            }

            // ================= GET SELECTED COURSE =================
            var selectedCourse = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == session.CourseId);

            // ================= EQUIPMENT VALIDATION =================
            // Get required equipment for selected course
            var requiredEquipments = await _context.CourseEquipmentRequirements
                .Where(c => c.CourseId == session.CourseId)
                .Select(c => c.EquipmentId)
                .ToListAsync();

            // Get equipment available in selected room
            var roomEquipments = await _context.RoomEquipments
                .Where(r => r.RoomId == session.RoomId)
                .Select(r => r.EquipmentId)
                .ToListAsync();

            // Check if room contains all required equipment
            bool hasAllRequired = requiredEquipments.All(req => roomEquipments.Contains(req));

            if (!hasAllRequired)
            {
                ModelState.AddModelError("RoomId", "Selected room does not meet course equipment requirements");
            }

            // Ensure selected course exists
            if (selectedCourse == null)
            {
                ModelState.AddModelError("", "Selected course is invalid");
            }

            // ================= INSTRUCTOR AVAILABILITY VALIDATION =================
            // Ensure instructor is available during selected time
            bool isAvailable = await _context.InstructorAvailabilities
                .AnyAsync(a =>
                    a.UserId == session.UserId &&
                    a.DayOfWeek == session.SessionDate.DayOfWeek.ToString() &&
                    a.StartTime <= session.StartTime &&
                    a.EndTime >= session.EndTime);

            if (!isAvailable)
                ModelState.AddModelError("", "Instructor is not available");

            // ================= INSTRUCTOR DOUBLE BOOKING VALIDATION =================
            // Prevent assigning instructor to overlapping sessions
            bool instructorConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.UserId == session.UserId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (instructorConflict)
                ModelState.AddModelError("", "Instructor already booked");

            // ================= ROOM DOUBLE BOOKING VALIDATION =================
            // Prevent assigning room to overlapping sessions
            bool roomConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.RoomId == session.RoomId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (roomConflict)
                ModelState.AddModelError("", "Room already booked");

            // ================= INSTRUCTOR EXPERTISE VALIDATION =================
            // Ensure instructor expertise matches course category
            if (selectedCourse != null)
            {
                bool hasMatchingExpertise = await _context.InstructorExpertises
                    .Include(i => i.Expertise)
                    .AnyAsync(i =>
                        i.UserId == session.UserId &&
                        i.Expertise.ExpertiseName == selectedCourse.Category.CategoryName);

                if (!hasMatchingExpertise)
                {
                    ModelState.AddModelError("", "Instructor expertise does not match the selected course category");
                }
            }

            // ================= SAVE SESSION =================
            if (ModelState.IsValid)
            {
                // Set available seats equal to session capacity
                session.AvailableSeats = session.SessionCapacity;

                // Set default session status
                session.Status = "Scheduled";

                // Save session into database
                _context.CourseSessions.Add(session);

                await _context.SaveChangesAsync();

                // ================= SEND NOTIFICATIONS =================
                // Notify instructor about assignment
                await _notification.CreateNotification(
                    session.UserId,
                    "You have been assigned to a new session"
                );

                // Load created session with related course
                var createdSession = await _context.CourseSessions
                    .Include(s => s.Course)
                    .FirstOrDefaultAsync(s => s.SessionId == session.SessionId);

                // Send course-specific notification
                if (createdSession != null)
                {
                    await _notification.CreateNotification(
                        createdSession.UserId,
                        $"You have been assigned to teach {createdSession.Course?.Title}"
                    );
                }

                // Redirect back to sessions list
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            LoadDropdowns();

            return View(session);
        }

        // ================= EDIT GET =================
        // Opens edit session page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find session by ID
            var session = await _context.CourseSessions.FindAsync(id);

            // Return 404 if not found
            if (session == null) return NotFound();

            // Load dropdowns
            LoadDropdowns();

            return View(session);
        }

        // ================= EDIT POST =================
        // Updates existing session
        [HttpPost]
        public async Task<IActionResult> Edit(CourseSession session)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Validate session
            ValidateSession(session);

            // Get selected course with category
            var selectedCourse = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.CourseId == session.CourseId);

            // Get required course equipment
            var requiredEquipments = await _context.CourseEquipmentRequirements
                .Where(c => c.CourseId == session.CourseId)
                .Select(c => c.EquipmentId)
                .ToListAsync();

            // Get room equipment
            var roomEquipments = await _context.RoomEquipments
                .Where(r => r.RoomId == session.RoomId)
                .Select(r => r.EquipmentId)
                .ToListAsync();

            // Validate room equipment requirements
            bool hasAllRequired = requiredEquipments.All(req => roomEquipments.Contains(req));

            if (!hasAllRequired)
            {
                ModelState.AddModelError("RoomId", "Selected room does not meet course equipment requirements");
            }

            // Ensure course exists
            if (selectedCourse == null)
            {
                ModelState.AddModelError("", "Selected course is invalid");
            }

            // Validate instructor availability
            bool isAvailable = await _context.InstructorAvailabilities
                .AnyAsync(a =>
                    a.UserId == session.UserId &&
                    a.DayOfWeek == session.SessionDate.DayOfWeek.ToString() &&
                    a.StartTime <= session.StartTime &&
                    a.EndTime >= session.EndTime);

            if (!isAvailable)
                ModelState.AddModelError("", "Instructor is not available");

            // Prevent instructor double booking
            bool instructorConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.SessionId != session.SessionId &&
                    s.UserId == session.UserId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (instructorConflict)
                ModelState.AddModelError("", "Instructor already booked");

            // Prevent room double booking
            bool roomConflict = await _context.CourseSessions
                .AnyAsync(s =>
                    s.SessionId != session.SessionId &&
                    s.RoomId == session.RoomId &&
                    s.SessionDate == session.SessionDate &&
                    s.StartTime < session.EndTime &&
                    session.StartTime < s.EndTime);

            if (roomConflict)
                ModelState.AddModelError("", "Room already booked");

            // Validate instructor expertise
            if (selectedCourse != null)
            {
                bool hasMatchingExpertise = await _context.InstructorExpertises
                    .Include(i => i.Expertise)
                    .AnyAsync(i =>
                        i.UserId == session.UserId &&
                        i.Expertise.ExpertiseName == selectedCourse.Category.CategoryName);

                if (!hasMatchingExpertise)
                {
                    ModelState.AddModelError("", "Instructor expertise does not match the course");
                }
            }

            // Save updates if validation passes
            if (ModelState.IsValid)
            {
                _context.CourseSessions.Update(session);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            LoadDropdowns();

            return View(session);
        }

        // ================= DELETE GET =================
        // Deletes session if no students are enrolled
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Load session with related course
            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            // Prevent deleting sessions with enrolled students
            bool hasEnrollments = await _context.Enrollments
                .AnyAsync(e => e.SessionId == id);

            if (hasEnrollments)
            {
                TempData["Error"] = "Cannot delete session because students are enrolled.";

                return RedirectToAction(nameof(Index));
            }

            // Remove session
            _context.CourseSessions.Remove(session);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Session deleted successfully";

            return RedirectToAction(nameof(Index));

            return View(session);
        }

        // ================= DELETE POST =================
        // Confirms session deletion
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Allow only Coordinator role
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            // Find session by ID
            var session = await _context.CourseSessions.FindAsync(id);

            // Delete session if found
            if (session != null)
            {
                _context.CourseSessions.Remove(session);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= SESSION VALIDATION =================
        // Validates session data
        private void ValidateSession(CourseSession session)
        {
            // Start time must be before end time
            if (session.StartTime >= session.EndTime)
                ModelState.AddModelError("", "Start time must be before end time");

            // Capacity must be greater than 0
            if (session.SessionCapacity <= 0)
                ModelState.AddModelError("", "Capacity must be greater than 0");
        }

        // ================= DROPDOWNS =================
        // Loads dropdown lists for Course, Room, and Instructor
        private void LoadDropdowns()
        {
            // Courses dropdown
            ViewBag.Courses = new SelectList(_context.Courses, "CourseId", "Title");

            // Rooms dropdown
            ViewBag.Rooms = new SelectList(_context.Rooms, "RoomId", "RoomName");

            // Instructors dropdown
            ViewBag.Instructors = new SelectList(
                _context.Users.Where(u => u.RoleId == 2),
                "UserId",
                "Name"
            );
        }
    }
}