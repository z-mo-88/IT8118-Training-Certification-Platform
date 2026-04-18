using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
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
    }
}