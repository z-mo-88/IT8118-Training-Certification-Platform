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
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorAvailability model)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            // Remove fields that are set in controller or are navigation properties
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (string.IsNullOrWhiteSpace(model.DayOfWeek))
                ModelState.AddModelError("DayOfWeek", "Please select a day");

            if (model.StartTime >= model.EndTime)
                ModelState.AddModelError("", "Start time must be before end time");

            int userId = UserId.Value;

            bool overlapExists = await _context.InstructorAvailabilities
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.DayOfWeek == model.DayOfWeek &&
                    a.StartTime < model.EndTime &&
                    model.StartTime < a.EndTime);

            if (overlapExists)
                ModelState.AddModelError("", "This availability overlaps with an existing time on the same day");

            if (ModelState.IsValid)
            {
                model.UserId = userId;
                model.IsAvailable = true;

                _context.InstructorAvailabilities.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var item = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == id && a.UserId == userId);

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

            var item = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == id && a.UserId == userId);

            if (item == null)
                return NotFound();

            _context.InstructorAvailabilities.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}