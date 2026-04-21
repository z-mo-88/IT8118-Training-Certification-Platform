using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.MVC.Services;

namespace TrainingSystem.MVC.Controllers
{
    public class InstructorExpertiseController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notification;


        public InstructorExpertiseController(AppDbContext context, NotificationService notification)
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

            var data = await _context.InstructorExpertises
                .Include(e => e.Expertise)
                .Where(e => e.UserId == targetUserId)
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

            LoadDropdown();
            ViewBag.TargetUserId = targetUserId;

            var model = new InstructorExpertise
            {
                UserId = targetUserId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorExpertise model)
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
            ModelState.Remove("Expertise");

            model.UserId = targetUserId;

            if (model.ExpertiseId == 0)
                ModelState.AddModelError("ExpertiseId", "Please select expertise");

            bool isInstructor = await _context.Users.AnyAsync(u => u.UserId == targetUserId && u.RoleId == 2);
            if (!isInstructor)
                ModelState.AddModelError("", "Invalid instructor.");

            bool exists = await _context.InstructorExpertises
                .AnyAsync(e => e.UserId == model.UserId && e.ExpertiseId == model.ExpertiseId);

            if (exists)
                ModelState.AddModelError("", "This expertise is already added");

            if (!ModelState.IsValid)
            {
                LoadDropdown();
                ViewBag.TargetUserId = targetUserId;
                return View(model);
            }

            _context.InstructorExpertises.Add(model);
            await _context.SaveChangesAsync();

           
            var expertise = await _context.ExpertiseAreas
                .FirstOrDefaultAsync(e => e.ExpertiseId == model.ExpertiseId);

           
            await _notification.CreateNotification(
                model.UserId,
                $"New expertise added: {expertise?.ExpertiseName}"
            );

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

            var item = await _context.InstructorExpertises
                .Include(i => i.Expertise)
                .FirstOrDefaultAsync(i => i.InstructorExpertiseId == id && i.UserId == targetUserId);

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

            var item = await _context.InstructorExpertises
                .FirstOrDefaultAsync(i => i.InstructorExpertiseId == id && i.UserId == targetUserId);

            if (item == null)
                return NotFound();

           

          
            var expertiseName = item.ExpertiseId;

            _context.InstructorExpertises.Remove(item);
            await _context.SaveChangesAsync();

            
            var expertise = await _context.ExpertiseAreas
                .FirstOrDefaultAsync(e => e.ExpertiseId == expertiseName);

           
            await _notification.CreateNotification(
                targetUserId,
                $"Expertise removed: {expertise?.ExpertiseName}"
            );

            return RedirectToAction(nameof(Index), new { userId = targetUserId });
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