using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class SessionsController : BaseController
    {
        private readonly AppDbContext _context;

        public SessionsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (RoleId != 2)
                return RedirectToAction("Login", "Account");

            int? instructorId = HttpContext.Session.GetInt32("UserId");

            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Where(s => s.UserId == instructorId.Value)
                .ToListAsync();

            return View(sessions);
        }
    }
}