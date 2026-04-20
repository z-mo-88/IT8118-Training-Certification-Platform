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

        public async Task<IActionResult> Index(int? userId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3)
            {
                if (userId == null)
                    return NotFound();

                targetUserId = userId.Value;
            }
            else if (roleId == 2)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            bool isInstructor = await _context.Users.AnyAsync(u => u.UserId == targetUserId && u.RoleId == 2);
            if (!isInstructor)
                return NotFound();

            var data = await _context.InstructorAvailabilities
                .Where(a => a.UserId == targetUserId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            ViewBag.TargetUserId = targetUserId;

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? userId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3)
            {
                if (userId == null)
                    return NotFound();

                targetUserId = userId.Value;
            }
            else if (roleId == 2)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            bool isInstructor = await _context.Users.AnyAsync(u => u.UserId == targetUserId && u.RoleId == 2);
            if (!isInstructor)
                return NotFound();

            ViewBag.TargetUserId = targetUserId;

            var model = new InstructorAvailability
            {
                UserId = targetUserId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorAvailability model)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3)
            {
                targetUserId = model.UserId;
                if (targetUserId <= 0)
                    return NotFound();
            }
            else if (roleId == 2)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove("User");

            if (string.IsNullOrWhiteSpace(model.DayOfWeek))
                ModelState.AddModelError("DayOfWeek", "Please select a day");

            if (model.StartTime >= model.EndTime)
                ModelState.AddModelError("", "Start time must be before end time");

            bool isInstructor = await _context.Users.AnyAsync(u => u.UserId == targetUserId && u.RoleId == 2);
            if (!isInstructor)
                ModelState.AddModelError("", "Invalid instructor.");

            bool overlapExists = await _context.InstructorAvailabilities
                .AnyAsync(a =>
                    a.UserId == targetUserId &&
                    a.DayOfWeek == model.DayOfWeek &&
                    a.StartTime < model.EndTime &&
                    model.StartTime < a.EndTime);

            if (overlapExists)
                ModelState.AddModelError("", "This availability overlaps with an existing time on the same day");

            if (!ModelState.IsValid)
            {
                ViewBag.TargetUserId = targetUserId;
                return View(model);
            }

            model.UserId = targetUserId;
            model.IsAvailable = true;

            _context.InstructorAvailabilities.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { userId = targetUserId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, int? userId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3)
            {
                if (userId == null)
                    return NotFound();

                targetUserId = userId.Value;
            }
            else if (roleId == 2)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var item = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == id && a.UserId == targetUserId);

            if (item == null)
                return NotFound();

            ViewBag.TargetUserId = targetUserId;
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3)
            {
                targetUserId = userId;
            }
            else if (roleId == 2)
            {
                targetUserId = currentUserId.Value;
            }
            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var item = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == id && a.UserId == targetUserId);

            if (item == null)
                return NotFound();

            _context.InstructorAvailabilities.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { userId = targetUserId });
        }
    }
}