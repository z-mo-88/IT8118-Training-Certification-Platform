using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorAvailabilityController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;

       

        public InstructorAvailabilityController(AppDbContext context, NotificationService notification)
        {
            _context = context;
            _notification = notification;
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

            await _notification.CreateNotification(
    model.UserId,
    $"New availability added: {model.DayOfWeek} {model.StartTime} - {model.EndTime}"
);

            return RedirectToAction(nameof(Index), new { userId = targetUserId });
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id, int userId)
        {
            var availability = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == id);

            if (availability == null)
                return NotFound();

            ViewBag.TargetUserId = userId;

            return View(availability);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstructorAvailability model)
        {


            int? roleId = HttpContext.Session.GetInt32("RoleId");
            int? currentUserId = UserId;

            if (roleId == null || currentUserId == null)
                return RedirectToAction("Login", "Account");

            int targetUserId;

            if (roleId == 3) 
            {
                targetUserId = model.UserId;
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

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var availability = await _context.InstructorAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == model.AvailabilityId && a.UserId == targetUserId);

            if (availability == null)
                return NotFound();

            availability.DayOfWeek = model.DayOfWeek;
            availability.StartTime = model.StartTime;
            availability.EndTime = model.EndTime;
            availability.IsAvailable = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { userId = targetUserId });
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

           

           
            var day = item.DayOfWeek;
            var start = item.StartTime;
            var end = item.EndTime;

            _context.InstructorAvailabilities.Remove(item);
            await _context.SaveChangesAsync();

            
            await _notification.CreateNotification(
                targetUserId,
                $"Availability removed: {day} {start} - {end}"
            );

            return RedirectToAction(nameof(Index), new { userId = targetUserId });
        }
    }
}