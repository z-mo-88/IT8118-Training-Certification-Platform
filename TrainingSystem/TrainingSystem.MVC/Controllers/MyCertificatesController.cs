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
            if (RoleId != 1)
                return RedirectToAction("Login", "Account");

            int? traineeId = UserId;

            var certificates = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .Where(c => c.UserId == traineeId.Value)
                .ToListAsync();

            return View(certificates);
        }
    }
}