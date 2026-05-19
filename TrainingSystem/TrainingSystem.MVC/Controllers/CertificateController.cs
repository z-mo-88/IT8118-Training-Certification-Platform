using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrainingSystem.API.Data;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificateController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly CertificatePdfService _pdf;

        public CertificateController(IHttpClientFactory factory, AppDbContext context)
        {
            _httpClient = factory.CreateClient("ApiClient");
            _pdf = new CertificatePdfService();
            _context = context;
        }

        // ================= DOWNLOAD CERTIFICATE PDF =================
        public async Task<IActionResult> Download(int id)
        {
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.CertificationTrack)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            if (certificate == null)
                return NotFound();

            var instructorName = "Ahmed Khalid";
            var duration = "20 Hours";

            var pdf = _pdf.Generate(
                certificate.User.Name,
                certificate.CertificationTrack.TrackName,
                certificate.CertificateReferenceNumber,
                certificate.IssuedDate.ToString(),
                instructorName,
                duration
            );

            return File(pdf, "application/pdf", "Certificate.pdf");
        }

        // ================= LOOKUP PAGE =================
        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        // ================= LOOKUP CERTIFICATE =================
        [HttpPost]
        public async Task<IActionResult> Lookup(string cpr, string reference)
        {
            // Save searched values
            ViewBag.SearchedCPR = cpr;
            ViewBag.SearchedReference = reference;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(cpr) || string.IsNullOrWhiteSpace(reference))
            {
                ViewBag.ResultMessage = "Please enter both CPR and certificate reference.";

                ViewBag.CertificateId = null;
                ViewBag.CompletedCourses = null;
                ViewBag.TraineeName = null;

                return View();
            }

            try
            {
                cpr = cpr.Trim();
                reference = reference.Trim();

                // Call API using CPR + Certificate Reference
                var response = await _httpClient.GetAsync(
                    $"api/Certificate/lookup?cpr={Uri.EscapeDataString(cpr)}&reference={Uri.EscapeDataString(reference)}");

                // Certificate not found
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ResultMessage = "No certificate was found for this CPR and certificate reference.";

                    ViewBag.CertificateId = null;
                    ViewBag.CompletedCourses = null;
                    ViewBag.TraineeName = null;

                    return View();
                }

                // Read API response
                var jsonData = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonData);

                var root = doc.RootElement;

                // Certificate information
                ViewBag.CertificateId = root.GetProperty("certificateId").GetInt32();

                ViewBag.CertificateReferenceNumber =
                    root.GetProperty("certificateReferenceNumber").GetString();

                // Certificate status formatting
                var status = root.GetProperty("certificateStatus").GetString();

                ViewBag.CertificateStatus = status switch
                {
                    "Pending" => "In Progress",
                    "pending" => "In Progress",

                    "Certified" => "Certified",
                    "certified" => "Certified",

                    "Eligible" => "Eligible",
                    "eligible" => "Eligible",

                    _ => status
                };

                // Additional certificate data
                ViewBag.IssuedDate = root.GetProperty("issuedDate").GetString();

                ViewBag.TrackName = root.GetProperty("trackName").GetString();

                ViewBag.CPR = root.GetProperty("cpr").GetString();

                // Trainee name
                ViewBag.TraineeName = root.TryGetProperty("traineeName", out var traineeNameElement)
                    ? traineeNameElement.GetString()
                    : "N/A";

                // ================= COMPLETED COURSES =================
                if (root.TryGetProperty("completedCourses", out JsonElement completedCoursesElement) &&
                    completedCoursesElement.ValueKind == JsonValueKind.Array)
                {
                    var completedCourses = new List<object>();

                    foreach (var course in completedCoursesElement.EnumerateArray())
                    {
                        completedCourses.Add(new
                        {
                            CourseId = course.TryGetProperty("courseId", out var courseIdElement)
                                ? courseIdElement.GetInt32()
                                : 0,

                            Title = course.TryGetProperty("title", out var titleElement)
                                ? titleElement.GetString()
                                : string.Empty,

                            IsRequired = course.TryGetProperty("isRequired", out var isRequiredElement) &&
                                         isRequiredElement.GetBoolean()
                        });
                    }

                    ViewBag.CompletedCourses = completedCourses;
                }
                else
                {
                    ViewBag.CompletedCourses = new List<object>();
                }

                ViewBag.ResultMessage = null;
            }
            catch
            {
                ViewBag.ResultMessage =
                    "Unable to connect to the server right now. Please try again.";

                ViewBag.CertificateId = null;
                ViewBag.CompletedCourses = null;
                ViewBag.TraineeName = null;
            }

            return View();
        }
    }
}