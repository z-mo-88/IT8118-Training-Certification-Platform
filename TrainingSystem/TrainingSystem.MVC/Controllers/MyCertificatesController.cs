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
            if (UserId == null)
                return RedirectToAction("Login", "Account");

            int userId = UserId.Value;

            // Get progress
            var progress = await _context.TraineeCertificationProgresses
                .Include(p => p.CertificationTrack)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // Get certificates separately
            var certificates = await _context.Certificates
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.Certificates = certificates;

            return View(progress);
        }
    }
}