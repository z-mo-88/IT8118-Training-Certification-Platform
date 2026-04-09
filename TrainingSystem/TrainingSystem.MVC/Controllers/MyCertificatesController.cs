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
            var auth = AuthorizeRole(1); 
            if (auth != null) return auth;

            int userId = UserId.Value;

            var certificates = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var progress = await _context.TraineeCertificationProgresses
                .Include(p => p.CertificationTrack)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            ViewBag.Progress = progress;

            return View(certificates);
        }
    }
}