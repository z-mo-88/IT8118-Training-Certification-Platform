using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorController : BaseController
    {
        private readonly AppDbContext _context;

        public InstructorController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var instructors = await _context.Users
                .Where(u => u.RoleId == 2)
                .Include(u => u.InstructorProfile)
                .Include(u => u.InstructorExpertises)
                    .ThenInclude(ie => ie.Expertise)
                .Include(u => u.InstructorAvailabilities)
                .ToListAsync();

            var model = instructors.Select(i => new InstructorDisplayViewModel
            {
                UserId = i.UserId,
                Name = i.Name,
                Email = i.Email,
                PhoneNumber = i.PhoneNumber ?? "",
                IsActive = i.IsActive,

                Bio = i.InstructorProfile != null ? i.InstructorProfile.Bio : "",
                Notes = i.InstructorProfile != null ? i.InstructorProfile.Notes : "",

                ExpertiseNames = i.InstructorExpertises?
                    .Where(e => e.Expertise != null)
                    .Select(e => e.Expertise.ExpertiseName)
                    .Distinct()
                    .ToList() ?? new List<string>(),

                AvailabilityText = i.InstructorAvailabilities?
                    .OrderBy(a => a.DayOfWeek)
                    .ThenBy(a => a.StartTime)
                    .Select(a => $"{a.DayOfWeek}: {a.StartTime} - {a.EndTime}")
                    .ToList() ?? new List<string>()
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var instructor = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.RoleId == 2);

            if (instructor == null)
                return NotFound();

            instructor.IsActive = !instructor.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = instructor.IsActive
                ? "Instructor activated successfully."
                : "Instructor deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var profile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound();

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(InstructorProfile model)
        {
            var auth = AuthorizeRole(2);
            if (auth != null) return auth;

            int userId = UserId.Value;

            var profile = await _context.InstructorProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound();

            profile.Bio = model.Bio;
            profile.Notes = model.Notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully";

            return RedirectToAction(nameof(Profile));
        }
    }
}