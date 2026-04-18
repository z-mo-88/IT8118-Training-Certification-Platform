using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;
using System.Security.Cryptography;
using System.Text;

namespace TrainingSystem.MVC.Controllers
{
    public class UsersController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

        public UsersController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();

            return View(users);
        }

        // ================= CREATE INSTRUCTOR (GET) =================
        [HttpGet]
        public IActionResult CreateInstructor()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        // ================= CREATE INSTRUCTOR (POST) =================
        [HttpPost]
        public async Task<IActionResult> CreateInstructor(User user, string Bio, string Notes)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            user.RoleId = 2; 
            user.IsActive = true;

            // Duplicate email check
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already exists");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);

                // Save user
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                //Create InstructorProfile
                var profile = new InstructorProfile
                {
                    UserId = user.UserId,
                    Bio = Bio,
                    Notes = Notes
                };

                _context.InstructorProfiles.Add(profile);
                await _context.SaveChangesAsync();

                //Notification
                await _notification.CreateNotification(
                    user.UserId,
                    "Your instructor account has been created"
                );

                TempData["Success"] = "Instructor created successfully";

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // ================= CREATE USER =================
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already exists");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _notification.CreateNotification(
                    user.UserId,
                    "Your account has been created"
                );

                return RedirectToAction(nameof(Index));
            }

            LoadRoles();
            return View(user);
        }

        // ================= EDIT =================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            LoadRoles();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;
                existingUser.IsActive = user.IsActive;

                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    existingUser.PasswordHash = HashPassword(user.PasswordHash);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadRoles();
            return View(user);
        }

        // ================= DELETE =================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= HELPERS =================
        private void LoadRoles()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "RoleName");
        }

        private void CleanModelState()
        {
            ModelState.Remove("Role");
            ModelState.Remove("Certificates");
            ModelState.Remove("CourseSessions");
            ModelState.Remove("Enrollments");
            ModelState.Remove("InstructorAvailabilities");
            ModelState.Remove("InstructorExpertises");
            ModelState.Remove("InstructorProfile");
            ModelState.Remove("Notifications");
            ModelState.Remove("TraineeCertificationProgresses");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}