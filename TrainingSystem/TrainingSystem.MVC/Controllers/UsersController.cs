using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;
using static Azure.Core.HttpHeader;

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

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already exists");
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.CPR == user.CPR))
            {
                ModelState.AddModelError("CPR", "CPR already exists");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 6 characters");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber) ||
                user.PhoneNumber.Length < 8)
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must be at least 8 digits");
            }

            if (string.IsNullOrWhiteSpace(user.CPR) || user.CPR.Length != 9)
            {
                ModelState.AddModelError("CPR", "CPR must be 9 digits");
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
        public async Task<IActionResult> Create(User user, string Bio, string Notes)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            user.RoleId = 1;
            user.IsActive = true;

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already exists");
            }
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            if (await _context.Users.AnyAsync(u => u.CPR == user.CPR))
            {
                ModelState.AddModelError("CPR", "CPR already exists");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash) || user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("PasswordHash", "Password must be at least 6 characters");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber) ||
                user.PhoneNumber.Length < 8)
            {
                ModelState.AddModelError("PhoneNumber", "Phone number must be at least 8 digits");
            }

            if (string.IsNullOrWhiteSpace(user.CPR) || user.CPR.Length != 9)
            {
                ModelState.AddModelError("CPR", "CPR must be 9 digits");
            }

            if (string.IsNullOrWhiteSpace(Bio))
            {
                ModelState.AddModelError("Bio", "Bio is required");
            }

            if (string.IsNullOrWhiteSpace(Notes))
            {
                ModelState.AddModelError("Notes", "Notes are required");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _notification.CreateNotification(
                    user.UserId,
                    "Your account has been created"
                );

                TempData["Success"] = "Trainee created successfully";

                return RedirectToAction(nameof(Index));
            }

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

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.Remove("PasswordHash");
            }
            else if (user.PasswordHash.Length < 6)
            {
                ModelState.AddModelError("PasswordHash",
                    "Password must be at least 6 characters");
            }

            if (string.IsNullOrWhiteSpace(user.CPR) || user.CPR.Length != 9)
            {
                ModelState.AddModelError("CPR", "CPR must be 9 digits");
            }

            bool cprExists = await _context.Users
                .AnyAsync(u => u.CPR == user.CPR && u.UserId != user.UserId);

            if (cprExists)
            {
                ModelState.AddModelError("CPR", "CPR already exists");
            }

            var existingUser = await _context.Users.FindAsync(user.UserId);

            if (existingUser == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.CPR = user.CPR;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;
                existingUser.IsActive = user.IsActive;

                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    existingUser.PasswordHash = HashPassword(user.PasswordHash);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "User updated successfully";

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

            var user = await _context.Users
                .Include(u => u.Notifications)
                .Include(u => u.Enrollments)
                .Include(u => u.TraineeCertificationProgresses)
                .Include(u => u.Certificates)
                .Include(u => u.InstructorProfile)
                .Include(u => u.InstructorExpertises)
                .Include(u => u.InstructorAvailabilities)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var hasSessions = await _context.CourseSessions
                .AnyAsync(s => s.UserId == id);

            if (hasSessions)
            {
                TempData["Error"] = "Cannot delete instructor assigned to sessions";
                return RedirectToAction(nameof(Index));
            }

            var hasEnrollments = await _context.Enrollments
                .AnyAsync(e =>
                    e.UserId == id &&
                    e.Status != "Dropped");

            if (hasEnrollments)
            {
                TempData["Error"] = "Cannot delete trainee with enrollments";
                return RedirectToAction(nameof(Index));
            }

            if (user.Notifications.Any())
                _context.Notifications.RemoveRange(user.Notifications);

            if (user.TraineeCertificationProgresses.Any())
                _context.TraineeCertificationProgresses.RemoveRange(user.TraineeCertificationProgresses);

            if (user.Certificates.Any())
                _context.Certificates.RemoveRange(user.Certificates);

            if (user.InstructorProfile != null)
                _context.InstructorProfiles.Remove(user.InstructorProfile);

            if (user.InstructorExpertises.Any())
                _context.InstructorExpertises.RemoveRange(user.InstructorExpertises);

            if (user.InstructorAvailabilities.Any())
                _context.InstructorAvailabilities.RemoveRange(user.InstructorAvailabilities);

            if (user.Enrollments.Any())
                _context.Enrollments.RemoveRange(user.Enrollments);

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully";

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