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

        public async Task<IActionResult> Index()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetInt32("roleId") != 3)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            int totalCourses = 0;
            int totalUsers = 0;
            int totalEnrollments = 0;
            int totalCertificates = 0;

            try
            {
                var coursesRes = await client.GetAsync("api/Courses");
                if (coursesRes.IsSuccessStatusCode)
                {
                    var courses = await coursesRes.Content.ReadFromJsonAsync<List<CourseViewModel>>();
                    totalCourses = courses?.Count ?? 0;
                }

                var usersRes = await client.GetAsync("api/Users");
                if (usersRes.IsSuccessStatusCode)
                {
                    var users = await usersRes.Content.ReadFromJsonAsync<List<UserViewModel>>();
                    totalUsers = users?.Count ?? 0;
                }

                var enrollmentsRes = await client.GetAsync("api/Enrollments");
                if (enrollmentsRes.IsSuccessStatusCode)
                {
                    var enrollments = await enrollmentsRes.Content.ReadFromJsonAsync<List<JsonElement>>();
                    totalEnrollments = enrollments?.Count ?? 0;
                }

                var certsRes = await client.GetAsync("api/Certificate");
                if (certsRes.IsSuccessStatusCode)
                {
                    var certs = await certsRes.Content.ReadFromJsonAsync<List<CertificateViewModel>>();
                    totalCertificates = certs?.Count ?? 0;
                }
            }
            catch { }

            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalEnrollments = totalEnrollments;
            ViewBag.TotalCertificates = totalCertificates;

            return View();
        }


        // ================= COURSES REPORT =================
        public async Task<IActionResult> Courses()
        {
            var client = CreateAuthorizedClient();

            if (client == null)
                return RedirectToAction("Login", "Auth");

            if (HttpContext.Session.GetInt32("roleId") != 3)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth"); //check role evrytime
            }

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

            if (HttpContext.Session.GetInt32("roleId") != 3)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth"); // check role everytime
            }

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

            if (HttpContext.Session.GetInt32("roleId") != 3)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth"); //check role everytime
            }

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

            if (HttpContext.Session.GetInt32("roleId") != 3)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth"); // check role everytime
            }

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

        // ================= EXPORT COURSES =================
        public async Task<IActionResult> ExportCourses()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");
            if (HttpContext.Session.GetInt32("roleId") != 3) { HttpContext.Session.Clear(); return RedirectToAction("Login", "Auth"); }

            var response = await client.GetAsync("api/Courses");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Courses");

            var data = await response.Content.ReadFromJsonAsync<List<CourseViewModel>>();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Course ID,Title,Description,Duration Hours");

            foreach (var c in data ?? new List<CourseViewModel>())
                csv.AppendLine($"{c.CourseId},{c.Title},{c.Description},{c.DurationHours}");

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Courses.csv");
        }

        // ================= EXPORT USERS =================
        public async Task<IActionResult> ExportUsers()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");
            if (HttpContext.Session.GetInt32("roleId") != 3) { HttpContext.Session.Clear(); return RedirectToAction("Login", "Auth"); }

            var response = await client.GetAsync("api/Users");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Users");

            var data = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("User ID,Name,Email,Phone,Role");

            foreach (var u in data ?? new List<UserViewModel>())
            {
                var role = u.RoleId == 1 ? "Trainee" : u.RoleId == 2 ? "Instructor" : u.RoleId == 3 ? "Coordinator" : "Unknown";
                csv.AppendLine($"{u.UserId},{u.Name},{u.Email},{u.PhoneNumber},{role}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Users.csv");
        }

        // ================= EXPORT ENROLLMENTS =================
        public async Task<IActionResult> ExportEnrollments()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");
            if (HttpContext.Session.GetInt32("roleId") != 3) { HttpContext.Session.Clear(); return RedirectToAction("Login", "Auth"); }

            var response = await client.GetAsync("api/Enrollments");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Enrollments");

            var jsonData = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Enrollment ID,Trainee,Course,Status,Enrollment Date,Outstanding Balance");

            foreach (var enrollment in jsonData ?? new List<JsonElement>())
            {
                string traineeName = "";
                string courseTitle = "";

                if (enrollment.TryGetProperty("user", out var user) && user.ValueKind != JsonValueKind.Null && user.TryGetProperty("name", out var name))
                    traineeName = name.GetString();

                if (enrollment.TryGetProperty("session", out var session) && session.ValueKind != JsonValueKind.Null &&
                    session.TryGetProperty("course", out var course) && course.ValueKind != JsonValueKind.Null &&
                    course.TryGetProperty("title", out var title))
                    courseTitle = title.GetString();

                var enrollmentId = enrollment.TryGetProperty("enrollmentId", out var eid) ? eid.GetInt32() : 0;
                var status = enrollment.TryGetProperty("status", out var s) ? s.GetString() : "";
                var date = enrollment.TryGetProperty("enrollmentDate", out var d) ? d.GetString() : "";
                var balance = enrollment.TryGetProperty("outstandingBalance", out var b) ? b.GetDecimal() : 0;

                csv.AppendLine($"{enrollmentId},{traineeName},{courseTitle},{status},{date},{balance}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Enrollments.csv");
        }

        // ================= EXPORT CERTIFICATES =================
        public async Task<IActionResult> ExportCertificates()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");
            if (HttpContext.Session.GetInt32("roleId") != 3) { HttpContext.Session.Clear(); return RedirectToAction("Login", "Auth"); }

            var response = await client.GetAsync("api/Certificate");
            if (!response.IsSuccessStatusCode) return RedirectToAction("Certificates");

            var data = await response.Content.ReadFromJsonAsync<List<CertificateViewModel>>();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Certificate ID,Reference Number,Trainee,Track,Status,Issued Date");

            foreach (var c in data ?? new List<CertificateViewModel>())
                csv.AppendLine($"{c.CertificateId},{c.CertificateReferenceNumber},{c.TraineeName},{c.TrackName},{c.CertificateStatus},{c.IssuedDate}");

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Certificates.csv");
        }
    }
}
