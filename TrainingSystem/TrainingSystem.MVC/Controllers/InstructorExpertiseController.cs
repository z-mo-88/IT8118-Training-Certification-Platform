using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorExpertiseController : BaseController
    {
        private readonly AppDbContext _context;

        public InstructorExpertiseController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var data = await _context.InstructorExpertises
                .Include(e => e.Expertise)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Expertise = new SelectList(_context.ExpertiseAreas, "ExpertiseId", "ExpertiseName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InstructorExpertise model)
        {
            model.UserId = UserId.Value;

            _context.InstructorExpertises.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InstructorExpertises.FindAsync(id);

            _context.InstructorExpertises.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}