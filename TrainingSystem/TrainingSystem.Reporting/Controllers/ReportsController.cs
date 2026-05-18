using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrainingSystem.Reporting.Models;

namespace TrainingSystem.Reporting.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory _factory;



        public ReportsController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
                return null;

            var client = _factory.CreateClient("ApiClient");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("token")))
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }


        // ================= COURSES REPORT =================
        public async Task<IActionResult> Courses()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync("api/Courses");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = response.StatusCode.ToString();
                return View(new List<CourseViewModel>());
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<CourseViewModel>>();

            return View(data ?? new List<CourseViewModel>());
        }

        // ================= USERS REPORT =================
        public async Task<IActionResult> Users()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync("api/Users");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = response.StatusCode.ToString();
                return View(new List<UserViewModel>());
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<UserViewModel>>();

            return View(data ?? new List<UserViewModel>());
        }

        // ================= ENROLLMENTS REPORT =================
        public async Task<IActionResult> Enrollments()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync("api/Enrollments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = response.StatusCode.ToString();
                return View(new List<EnrollmentViewModel>());
            }

            var jsonData = await response.Content
                .ReadFromJsonAsync<List<JsonElement>>();

            var enrollments = jsonData.Select(enrollment =>
            {
                string traineeName = "";
                string courseTitle = "";

                if (enrollment.TryGetProperty("user", out var user) &&
                    user.ValueKind != JsonValueKind.Null &&
                    user.TryGetProperty("name", out var name))
                {
                    traineeName = name.GetString();
                }

                if (enrollment.TryGetProperty("session", out var session) &&
                    session.ValueKind != JsonValueKind.Null &&
                    session.TryGetProperty("course", out var course) &&
                    course.ValueKind != JsonValueKind.Null &&
                    course.TryGetProperty("title", out var title))
                {
                    courseTitle = title.GetString();
                }

                return new EnrollmentViewModel
                {
                    EnrollmentId = enrollment.TryGetProperty("enrollmentId", out var enrollmentId)
                        ? enrollmentId.GetInt32()
                        : 0,

                    TraineeName = traineeName,

                    CourseTitle = courseTitle,

                    Status = enrollment.TryGetProperty("status", out var status)
                        ? status.GetString()
                        : "",

                    EnrollmentDate = enrollment.TryGetProperty("enrollmentDate", out var enrollmentDate)
                        ? enrollmentDate.GetString()
                        : "",

                    OutstandingBalance = enrollment.TryGetProperty("outstandingBalance", out var balance)
                        ? balance.GetDecimal()
                        : 0
                };
            }).ToList();

            return View(enrollments);
        }

        // ================= CERTIFICATES REPORT =================
        public async Task<IActionResult> Certificates()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync("api/Certificate");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = response.StatusCode.ToString();
                return View(new List<CertificateViewModel>());
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<CertificateViewModel>>();

            return View(data ?? new List<CertificateViewModel>());
        }
    }
}
