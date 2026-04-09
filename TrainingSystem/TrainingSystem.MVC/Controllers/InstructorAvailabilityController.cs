using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorAvailabilityController : BaseController
    {
        private readonly AppDbContext _context;

        public InstructorAvailabilityController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var data = await _context.InstructorAvailabilities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return View(data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InstructorAvailability model)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            if (model.StartTime >= model.EndTime)
                ModelState.AddModelError("", "Invalid time");

            if (ModelState.IsValid)
            {
                model.UserId = UserId.Value;
                model.IsAvailable = true;

                _context.InstructorAvailabilities.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.InstructorAvailabilities.FindAsync(id);

            _context.InstructorAvailabilities.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}