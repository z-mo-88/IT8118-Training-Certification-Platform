using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int traineeId = 1; // temporary (later we use login)

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == traineeId)
                .ToListAsync();

            return View(enrollments);
        }
    }
}