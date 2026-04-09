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

        //  MY SESSIONS 
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(2); 
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Where(s => s.UserId == instructorId)
                .ToListAsync();

            return View(sessions);
        }

        //  SESSION DETAILS 
        public async Task<IActionResult> Details(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null)
                return NotFound();

            return View(session);
        }
    }
}