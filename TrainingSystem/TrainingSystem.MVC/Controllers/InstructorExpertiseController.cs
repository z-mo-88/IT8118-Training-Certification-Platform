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

        // GET
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdown();
            return View();
        }

        // POST
        [HttpPost]
        public async Task<IActionResult> Create(InstructorExpertise model)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            model.UserId = UserId.Value;

            
            bool exists = await _context.InstructorExpertises
                .AnyAsync(e => e.UserId == model.UserId && e.ExpertiseId == model.ExpertiseId);

            if (exists)
            {
                ModelState.AddModelError("", "This expertise already added");
                LoadDropdown();
                return View(model);
            }

          
            if (model.ExpertiseId == 0)
            {
                ModelState.AddModelError("", "Please select expertise");
                LoadDropdown();
                return View(model);
            }

            _context.InstructorExpertises.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InstructorExpertises.FindAsync(id);

            if (item != null)
            {
                _context.InstructorExpertises.Remove(item);
                await _context.SaveChangesAsync();
            }

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