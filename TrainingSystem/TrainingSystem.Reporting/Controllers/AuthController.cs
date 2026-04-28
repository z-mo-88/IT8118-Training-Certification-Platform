using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using TrainingSystem.Reporting.Models;
using System.Net.Http.Json;

public class AuthController : Controller
{
    private readonly HttpClient _httpClient;

    public AuthController()
    {
        _httpClient = new HttpClient();
    }

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
            "https://localhost:7258/api/Auth/login", // YOUR API
            loginData
        );

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            HttpContext.Session.SetString("token", result.Token);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Invalid login";
        return View();
    }
}