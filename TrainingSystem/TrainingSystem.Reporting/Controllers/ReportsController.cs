using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrainingSystem.Reporting.Models;

public class ReportsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> Courses()
    {
        var token = HttpContext.Session.GetString("token");

        var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://localhost:7258/api/Courses");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = response.StatusCode.ToString();
            return View(new List<CourseViewModel>());
        }

        var data = await response.Content.ReadFromJsonAsync<List<CourseViewModel>>();

        return View(data ?? new List<CourseViewModel>());
    }

    public async Task<IActionResult> Users()
    {
        var token = HttpContext.Session.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://localhost:7258/api/Users");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = response.StatusCode.ToString();
            return View(new List<System.Text.Json.JsonElement>());
        }

        var data = await response.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();

        return View(data ?? new List<System.Text.Json.JsonElement>());
    }

    public async Task<IActionResult> Enrollments()
    {
        var token = HttpContext.Session.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://localhost:7258/api/Enrollments");

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = response.StatusCode.ToString();
            ViewBag.ErrorDetails = errorDetails;
            return View(new List<EnrollmentViewModel>());
        }

        var jsonData = await response.Content.ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();

        var enrollments = jsonData.Select(enrollment =>
        {
            string traineeName = "";
            string courseTitle = "";

            if (enrollment.TryGetProperty("user", out var user) &&
                user.ValueKind != System.Text.Json.JsonValueKind.Null &&
                user.TryGetProperty("name", out var name))
            {
                traineeName = name.GetString();
            }

            if (enrollment.TryGetProperty("session", out var session) &&
                session.ValueKind != System.Text.Json.JsonValueKind.Null &&
                session.TryGetProperty("course", out var course) &&
                course.ValueKind != System.Text.Json.JsonValueKind.Null &&
                course.TryGetProperty("title", out var title))
            {
                courseTitle = title.GetString();
            }

            return new EnrollmentViewModel
            {
                EnrollmentId = enrollment.TryGetProperty("enrollmentId", out var enrollmentId) ? enrollmentId.GetInt32() : 0,
                TraineeName = traineeName,
                CourseTitle = courseTitle,
                Status = enrollment.TryGetProperty("status", out var status) ? status.GetString() : "",
                EnrollmentDate = enrollment.TryGetProperty("enrollmentDate", out var enrollmentDate) ? enrollmentDate.GetString() : "",
                OutstandingBalance = enrollment.TryGetProperty("outstandingBalance", out var balance) ? balance.GetDecimal() : 0
            };
        }).ToList();

        return View(enrollments);
    }

    public async Task<IActionResult> Certificates()
    {
        var token = HttpContext.Session.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("https://localhost:7258/api/Certificate");

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = response.StatusCode.ToString();
            ViewBag.ErrorDetails = errorDetails;
            return View(new List<CertificateViewModel>());
        }

        var data = await response.Content.ReadFromJsonAsync<List<CertificateViewModel>>();

        return View(data ?? new List<CertificateViewModel>());
    }
}