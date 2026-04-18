using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class MyCertificatesController : BaseController
    {
        private readonly AppDbContext _context;

        public MyCertificatesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(1); // Trainee role
            if (auth != null) return auth;

            // ✅ FIX: Safe UserId extraction (no warning, no crash)
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim.Value);

            // ✅ Get Certificates
            var certificates = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // ✅ Get Progress
            var progress = await _context.TraineeCertificationProgresses
                .Include(p => p.CertificationTrack)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // ✅ FIX: Prevent progress > 100%
            foreach (var p in progress)
            {
                p.ProgressPercent = Math.Min(p.ProgressPercent, 100);
            }

            // ✅ Send to View
            ViewBag.Progress = progress;

            return View(certificates);
        }
    }
}