using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class UsersController : BaseController
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3); // Coordinator
            if (auth != null) return auth;

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // CREATE 
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await new NotificationController(_context)
                    .CreateNotification(user.UserId, "Your account has been created");

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // EDIT 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            CleanModelState();

            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // DELETE 
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void CleanModelState()
        {
            ModelState.Remove("Role");
            ModelState.Remove("Certificates");
            ModelState.Remove("CourseSessions");
            ModelState.Remove("Enrollments");
            ModelState.Remove("InstructorAvailabilities");
            ModelState.Remove("InstructorExpertises");
            ModelState.Remove("InstructorProfile");
            ModelState.Remove("Notifications");
            ModelState.Remove("TraineeCertificationProgresses");
        }
    }
}