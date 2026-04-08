using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class EnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;

        public EnrollmentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (RoleId != 1)
                return RedirectToAction("Login", "Account");

            int? traineeId = HttpContext.Session.GetInt32("UserId");

            var enrollments = await _context.Enrollments
                .Include(e => e.Session)
                .ThenInclude(s => s.Course)
                .Where(e => e.UserId == traineeId.Value)
                .ToListAsync();

            return View(enrollments);
        }


    }
}