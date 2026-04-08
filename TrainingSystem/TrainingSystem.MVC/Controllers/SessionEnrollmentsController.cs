using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers

{

    public class SessionEnrollmentsController : Controller

    {

        private readonly AppDbContext _context;

        public SessionEnrollmentsController(AppDbContext context)

        {

            _context = context;

        }

        public async Task<IActionResult> Index(int id)

        {

            var enrollments = await _context.Enrollments

                .Include(e => e.User)

                .Where(e => e.SessionId == id)

                .ToListAsync();

            ViewBag.SessionId = id;

            return View(enrollments);

        }

    }

}
