using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Helpers;

namespace TrainingSystem.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectUserByRole();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectUserByRole();
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password are required";
                return View();
            }

            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

await _context.SaveChangesAsync();

            return RedirectUserByRole();
        }

        // REGISTER
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectUserByRole();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, int roleId)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectUserByRole();
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = HashPassword(password),
                RoleId = roleId,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ROLE-BASED REDIRECT
        private IActionResult RedirectUserByRole()
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");

            switch (roleId)
            {
                case 1: // Trainee
                    return RedirectToAction("Index", "Enrollments");

                case 2: // Instructor
                    return RedirectToAction("Index", "InstructorSessions");

                case 3: // Coordinator
                    return RedirectToAction("Index", "Users");

                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // HASH FUNCTION
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