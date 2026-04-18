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
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            LoadDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorExpertise model)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Expertise");

            model.UserId = UserId.Value;

            if (model.ExpertiseId == 0)
            {
                ModelState.AddModelError("ExpertiseId", "Please select expertise");
            }

            bool exists = await _context.InstructorExpertises
                .AnyAsync(e => e.UserId == model.UserId && e.ExpertiseId == model.ExpertiseId);

            if (exists)
            {
                ModelState.AddModelError("", "This expertise is already added");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdown();
                return View(model);
            }

            _context.InstructorExpertises.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var item = await _context.InstructorExpertises
                .Include(i => i.Expertise)
                .FirstOrDefaultAsync(i => i.InstructorExpertiseId == id && i.UserId == userId);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var item = await _context.InstructorExpertises
                .FirstOrDefaultAsync(i => i.InstructorExpertiseId == id && i.UserId == userId);

            if (item == null)
                return NotFound();

            _context.InstructorExpertises.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdown()
        {
            ViewBag.Expertise = new SelectList(
                _context.ExpertiseAreas,
                "ExpertiseId",
                "ExpertiseName"
            );
        }
    }
}