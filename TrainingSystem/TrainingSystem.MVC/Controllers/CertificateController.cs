using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TrainingSystem.API.Data;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    // Controller responsible for certificate lookup and certificate PDF download
    // This controller supports:
    // 1. Public certificate verification
    // 2. Certificate PDF generation
    // 3. API communication using HttpClient
    public class CertificateController : Controller
    {
        // HttpClient used to call the API
        private readonly HttpClient _httpClient;

        // Database context
        private readonly AppDbContext _context;

        // Service used to generate PDF certificates
        private readonly CertificatePdfService _pdf;

        // Constructor injection
        public CertificateController(IHttpClientFactory factory, AppDbContext context)
        {
            // Create HttpClient from configured API client
            _httpClient = factory.CreateClient("ApiClient");

            // Create PDF service
            _pdf = new CertificatePdfService();

            // Database context
            _context = context;
        }

        // ================= DOWNLOAD CERTIFICATE PDF =================
        // Generates and downloads certificate as PDF file
        public async Task<IActionResult> Download(int id)
        {
            // Load certificate with trainee and certification track data
            var certificate = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.CertificationTrack)
                .FirstOrDefaultAsync(c => c.CertificateId == id);

            // Return 404 if certificate not found
            if (certificate == null)
                return NotFound();

            // Sample instructor name and duration for certificate
            // These values are used inside the PDF
            var instructorName = "Ahmed Khalid";
            var duration = "20 Hours";

            // Generate PDF file
            var pdf = _pdf.Generate(
                certificate.User.Name,
                certificate.CertificationTrack.TrackName,
                certificate.CertificateReferenceNumber,
                certificate.IssuedDate.ToString(),
                instructorName,
                duration
            );

            // Return downloadable PDF file
            return File(pdf, "application/pdf", "Certificate.pdf");
        }

        // ================= LOOKUP GET =================
        // Opens certificate lookup page
        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        // ================= LOOKUP POST =================
        // Searches for certificate using trainee ID and reference number
        [HttpPost]
        public async Task<IActionResult> Lookup(int userId, string reference)
        {
            // Save entered values into ViewBag
            ViewBag.SearchedUserId = userId;
            ViewBag.SearchedReference = reference;

            // Validate inputs
            if (userId <= 0 || string.IsNullOrWhiteSpace(reference))
            {
                ViewBag.ResultMessage = "Please enter both trainee ID and certificate reference.";

                ViewBag.CertificateId = null;
                ViewBag.CompletedCourses = null;
                ViewBag.TraineeName = null;

                return View();
            }

            try
            {
                // Remove extra spaces
                reference = reference.Trim();

                // ================= API CALL =================
                // Call API endpoint using HttpClient
                var response = await _httpClient.GetAsync(
                    $"api/Certificate/lookup?userId={userId}&reference={Uri.EscapeDataString(reference)}");

                // If API returns error or certificate not found
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ResultMessage = "No certificate was found for this trainee ID and certificate reference.";

                    ViewBag.CertificateId = null;
                    ViewBag.CompletedCourses = null;
                    ViewBag.TraineeName = null;

                    return View();
                }

                // Read JSON response from API
                var jsonData = await response.Content.ReadAsStringAsync();

                // Parse JSON data
                using JsonDocument doc = JsonDocument.Parse(jsonData);

                var root = doc.RootElement;

                // ================= CERTIFICATE DATA =================
                // Store certificate data inside ViewBag
                ViewBag.CertificateId = root.GetProperty("certificateId").GetInt32();

                ViewBag.CertificateReferenceNumber =
                    root.GetProperty("certificateReferenceNumber").GetString();

                // Get raw certificate status from API
                var status = root.GetProperty("certificateStatus").GetString();

                // Convert API statuses into user-friendly display values
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

                // Store additional certificate information
                ViewBag.IssuedDate = root.GetProperty("issuedDate").GetString();

                ViewBag.TrackName = root.GetProperty("trackName").GetString();

                // ================= TRAINEE NAME =================
                // Safely retrieve trainee name if available
                ViewBag.TraineeName = root.TryGetProperty("traineeName", out var traineeNameElement)
                    ? traineeNameElement.GetString()
                    : "N/A";

                // ================= COMPLETED COURSES =================
                // Read completed courses array from API response
                if (root.TryGetProperty("completedCourses", out JsonElement completedCoursesElement) &&
                    completedCoursesElement.ValueKind == JsonValueKind.Array)
                {
                    // Create list to store completed courses
                    var completedCourses = new List<object>();

                    // Loop through each completed course
                    foreach (var course in completedCoursesElement.EnumerateArray())
                    {
                        completedCourses.Add(new
                        {
                            // Course ID
                            CourseId = course.TryGetProperty("courseId", out var courseIdElement)
                                ? courseIdElement.GetInt32()
                                : 0,

                            // Course title
                            Title = course.TryGetProperty("title", out var titleElement)
                                ? titleElement.GetString()
                                : string.Empty,

                            // Required or optional course
                            IsRequired = course.TryGetProperty("isRequired", out var isRequiredElement) &&
                                         isRequiredElement.GetBoolean()
                        });
                    }

                    // Send completed courses to the view
                    ViewBag.CompletedCourses = completedCourses;
                }
                else
                {
                    // Empty course list if no completed courses exist
                    ViewBag.CompletedCourses = new List<object>();
                }

                // Clear error message
                ViewBag.ResultMessage = null;
            }
            catch
            {
                // Error handling if API/server fails
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