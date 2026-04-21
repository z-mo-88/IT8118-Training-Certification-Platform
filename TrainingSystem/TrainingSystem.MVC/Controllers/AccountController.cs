using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

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
                return RedirectByRole();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectByRole();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and Password required";
                return View();
            }

            string hashed = HashPassword(password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashed);

            if (user == null)
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            return RedirectByRole();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // REDIRECT BY ROLE
        private IActionResult RedirectByRole()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");

            return roleId switch
            {
                1 => RedirectToAction("Index", "Enrollments"),
                2 => RedirectToAction("Index", "InstructorSessions"),
                3 => RedirectToAction("Index", "Users"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // HASH
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}