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

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

     var users = await _context.Users
    .Include(u => u.Role)
    .ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadRoles();
            return View();
        }
        [HttpGet]
        public IActionResult CreateInstructor()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = new User
            {
                RoleId = 2,
                IsActive = true
            };

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInstructor(User user)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            user.RoleId = 2;
            user.IsActive = true;

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already exists");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _notification.Create(user.UserId, "Your instructor account has been created");

                return RedirectToAction(nameof(Index));
            }

            return View(user);
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

                await _notification.Create(user.UserId, "Your account has been created");

                return RedirectToAction(nameof(Index));
            }

            LoadRoles();
            return View(user);
        }

        [HttpGet]

        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

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
            if (existingUser == null)
                return NotFound();

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

        [HttpGet]

        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

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