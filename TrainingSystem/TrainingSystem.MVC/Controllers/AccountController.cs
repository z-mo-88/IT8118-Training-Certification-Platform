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
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
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

            // SESSION
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("UserName", user.Name);

            // COOKIE 
            var cookieOptions = new CookieOptions
            {
                Expires = rememberMe
          ? DateTimeOffset.Now.AddDays(7)
          : DateTimeOffset.Now.AddMinutes(60),
                HttpOnly = true,
                IsEssential = true
            };

            Response.Cookies.Append("UserId", user.UserId.ToString(), cookieOptions);
            Response.Cookies.Append("RoleId", user.RoleId.ToString(), cookieOptions);
            Response.Cookies.Append("UserName", user.Name, cookieOptions);

            return RedirectByRole();
        }

        // Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string cpr, string email, string phoneNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ViewBag.Error = "Name is required";
                return View();
            }

            if (string.IsNullOrWhiteSpace(cpr) ||
                cpr.Length != 9 ||
                !cpr.All(char.IsDigit))
            {
                ViewBag.CPRError = "CPR must be 9 digits";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ViewBag.PasswordError = "Password must be at least 6 characters";
                return View();
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) ||
                phoneNumber.Length != 8 ||
                !phoneNumber.All(char.IsDigit))
            {
                ViewBag.PhoneError = "Phone number must be 8 digits";
                return View();
            }

            var exists = await _context.Users.AnyAsync(u => u.Email == email);

            if (exists)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var cprExists = await _context.Users.AnyAsync(u => u.CPR == cpr);

            if (cprExists)
            {
                ViewBag.CPRError = "CPR already exists";
                return View();
            }

            var user = new User
            {
                Name = name,
                CPR = cpr,
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = HashPassword(password),
                RoleId = 1,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully!";

            return RedirectToAction("Login");
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // DELETE COOKIES
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("RoleId");
            Response.Cookies.Delete("UserName");

            return RedirectToAction("Login");
        }

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

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}