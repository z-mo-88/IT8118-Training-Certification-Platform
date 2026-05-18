using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TrainingSystem.Reporting.Models;

namespace TrainingSystem.Reporting.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public AuthController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _factory.CreateClient("ApiClient");

            var loginData = new
            {
                Email = email,
                Password = password
            };

            var response = await client.PostAsJsonAsync(
                "api/Auth/login",
                loginData
            );

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ViewBag.Error = "Invalid login";
                return View();
            }

            // Restrict to coordinator role
            if (result.RoleId != 3)
            {
                ViewBag.Error = "Access denied. Reporting app is restricted.";
                return View();
            }

            HttpContext.Session.SetString("token", result.Token);
            HttpContext.Session.SetInt32("roleId", result.RoleId);
            HttpContext.Session.SetInt32("userId", result.UserId);

            return RedirectToAction("Index", "Reports");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
