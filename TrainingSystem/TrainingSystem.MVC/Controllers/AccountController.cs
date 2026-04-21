using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        //  LOGIN 
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectUserByRole();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
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

            // SESSION 
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);

            //  COOKIE AUTH 
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTime.UtcNow.AddDays(7)
                    : DateTime.UtcNow.AddMinutes(30)
            });

            return RedirectUserByRole();
        }

        //  REGISTER 
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectUserByRole();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string email, string password, int roleId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
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

        //  LOGOUT 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        // ROLE REDIRECT 
        private IActionResult RedirectUserByRole()
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(roleClaim))
            {
                return RedirectToAction("Login");
            }

            int roleId = int.Parse(roleClaim);

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
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}