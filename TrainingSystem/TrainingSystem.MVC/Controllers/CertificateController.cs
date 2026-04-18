using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificateController : Controller
    {
        private readonly HttpClient _httpClient;

        public CertificateController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("ApiClient");
        }

        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Lookup(int userId, string reference)
        {
            ViewBag.SearchedUserId = userId;
            ViewBag.SearchedReference = reference;

            if (userId <= 0 || string.IsNullOrWhiteSpace(reference))
            {
                ViewBag.ResultMessage = "Please enter valid trainee ID and certificate reference.";
                return View();
            }

            try
            {
                reference = reference.Trim();

                var response = await _httpClient.GetAsync(
                    $"api/Certificate/lookup?userId={userId}&reference={Uri.EscapeDataString(reference)}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ResultMessage = "Certificate not found";

                    // 🔥 Clear old data
                    ViewBag.CertificateId = null;
                    ViewBag.CompletedCourses = null;

                    return View();
                }

                var jsonData = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                ViewBag.CertificateId = root.GetProperty("certificateId").GetInt32();
                ViewBag.CertificateReferenceNumber = root.GetProperty("certificateReferenceNumber").GetString();
                ViewBag.CertificateStatus = root.GetProperty("certificateStatus").GetString();
                ViewBag.IssuedDate = root.GetProperty("issuedDate").GetString();
                ViewBag.TrackName = root.GetProperty("trackName").GetString();

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
                ViewBag.ResultMessage = "Error connecting to server";

                // 🔥 Clear old data
                ViewBag.CertificateId = null;
                ViewBag.CompletedCourses = null;
            }

            return View();
        }
    }
}