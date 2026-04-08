using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers

{

    public class MyCertificatesController : Controller

    {

        private readonly AppDbContext _context;

        public MyCertificatesController(AppDbContext context)

        {

            _context = context;

        }

        public async Task<IActionResult> Index()

        {

            int? traineeId = HttpContext.Session.GetInt32("UserId");

            if (traineeId == null)

                return RedirectToAction("Login", "Account");

            var certificates = await _context.Certificates

                .Include(c => c.CertificationTrack)

                .Where(c => c.UserId == traineeId.Value)

                .ToListAsync();

            return View(certificates);

        }

    }

}
