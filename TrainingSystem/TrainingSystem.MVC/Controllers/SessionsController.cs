using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers

{

    public class SessionsController : Controller

    {

        private readonly AppDbContext _context;

        public SessionsController(AppDbContext context)

        {

            _context = context;

        }

        public async Task<IActionResult> Index()

        {

            int? instructorId = HttpContext.Session.GetInt32("UserId");

            if (instructorId == null)

                return RedirectToAction("Login", "Account");

            var sessions = await _context.CourseSessions

                .Include(s => s.Course)

                .Include(s => s.Room)

                .Where(s => s.UserId == instructorId.Value)

                .ToListAsync();

            return View(sessions);

        }

    }

}
