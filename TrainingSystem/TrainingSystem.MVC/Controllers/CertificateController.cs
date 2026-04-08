using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificateController : Controller
    {
        private readonly HttpClient _httpClient;

        public CertificateController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7258/");
        }

        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Lookup(int userId, string reference)
        {
            var response = await _httpClient.GetAsync(
                $"api/Certificate/lookup?userId={userId}&reference={reference}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;

                ViewBag.CertificateId = root.GetProperty("certificateId").GetInt32();
                ViewBag.CertificateReferenceNumber = root.GetProperty("certificateReferenceNumber").GetString();
                ViewBag.CertificateStatus = root.GetProperty("certificateStatus").GetString();
                ViewBag.IssuedDate = root.GetProperty("issuedDate").GetString();
                ViewBag.TrackName = root.GetProperty("trackName").GetString();
                ViewBag.ResultMessage = null;
            }
            else
            {
                ViewBag.CertificateId = null;
                ViewBag.CertificateReferenceNumber = null;
                ViewBag.CertificateStatus = null;
                ViewBag.IssuedDate = null;
                ViewBag.TrackName = null;
                ViewBag.ResultMessage = "Certificate not found";
            }

            return View();
        }
    }
}