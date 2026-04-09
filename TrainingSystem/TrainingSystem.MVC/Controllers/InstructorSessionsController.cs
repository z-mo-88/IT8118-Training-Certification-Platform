using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorSessionsController : BaseController
    {
        private readonly AppDbContext _context;

        public InstructorSessionsController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(2); 
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var sessions = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .Where(s => s.UserId == instructorId)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return View(sessions);
        }

       
        public async Task<IActionResult> Details(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int instructorId = UserId.Value;

            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s =>
                    s.SessionId == id &&
                    s.UserId == instructorId); 

            if (session == null)
                return NotFound();

            return View(session);
        }
    }
}