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

        //  GET 
        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        //  POST 
        [HttpPost]
        public async Task<IActionResult> Lookup(int userId, string reference)
        {
            if (userId <= 0 || string.IsNullOrEmpty(reference))
            {
                ViewBag.ResultMessage = "Please enter valid data";
                return View();
            }

            try
            {
                var response = await _httpClient.GetAsync(
                    $"api/Certificate/lookup?userId={userId}&reference={reference}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ResultMessage = "Certificate not found";
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

                ViewBag.ResultMessage = null;
            }
            catch
            {
                ViewBag.ResultMessage = "Error connecting to server";
            }

            return View();
        }
    }
}