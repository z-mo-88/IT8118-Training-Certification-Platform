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

            // Use Session instead of Claims
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var certificates = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();

            var progress = await _context.TraineeCertificationProgresses
                .Include(p => p.CertificationTrack)
                .Where(p => p.UserId == userId.Value)
                .ToListAsync();

            foreach (var p in progress)
            {
                p.ProgressPercent = Math.Min(p.ProgressPercent, 100);
            }

            ViewBag.Progress = progress;

            return View(certificates);
        }
    }
}