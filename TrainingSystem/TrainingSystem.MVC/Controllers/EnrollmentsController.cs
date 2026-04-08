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
            int traineeId = 1;

            var enrollments = await _context.Enrollments
              .Include(e => e.Session)
              .ThenInclude(cs => cs.Course)
              .Where(e => e.UserId == traineeId)
              .ToListAsync();

            return View(enrollments);
        }
    }
}