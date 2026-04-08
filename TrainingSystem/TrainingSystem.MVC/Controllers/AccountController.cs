
using Microsoft.AspNetCore.Mvc;

namespace TrainingSystem.MVC.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(int userId, string role)
        {
            HttpContext.Session.SetInt32("UserId", userId);
            HttpContext.Session.SetString("Role", role);

            if (role == "Trainee")
                return RedirectToAction("Index", "Enrollments");

            if (role == "Instructor")
                return RedirectToAction("Index", "Sessions");

            if (role == "Coordinator")
                return RedirectToAction("Index", "Users");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

