using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TrainingSystem.Reporting.Models;

public class AuthController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = new HttpClient();

        var loginData = new
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync(
            "https://localhost:7258/api/Auth/login",
            loginData
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                HttpContext.Session.SetString("token", result.Token);
                return RedirectToAction("Index", "Reports");
            }
        }

        ViewBag.Error = "Invalid login";
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("token");
        HttpContext.Session.Clear();

        return Redirect("/Auth/Login");
    }
}