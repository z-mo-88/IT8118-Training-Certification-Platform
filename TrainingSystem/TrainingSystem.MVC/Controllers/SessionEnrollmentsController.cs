using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class SessionEnrollmentsController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

        public SessionEnrollmentsController(AppDbContext context)
        {
            _context = context;
            _notification = new NotificationService(_context);
        }

        //  VIEW STUDENTS 
        public async Task<IActionResult> Index(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.AssessmentResults)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .Where(e => e.SessionId == id && e.Status != "Dropped")
                .ToListAsync();

            ViewBag.SessionId = id;

            return View(enrollments);
        }

        // ================= MARK ATTENDING =================
        public async Task<IActionResult> MarkAttending(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            enrollment.Status = "Attending";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = enrollment.SessionId });
        }

       
       
    }
}