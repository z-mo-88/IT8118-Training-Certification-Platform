using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

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
                .ToListAsync();

            return View(instructors);
        }
    }
}