using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.MVC.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetInt32("RoleId");

          
            if (role != 1 && role != 2)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = UserId.Value;

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        //  MARK AS READ 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");

            if (role != 1 && role != 2)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = UserId.Value;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification marked as read.";

            return RedirectToAction(nameof(Index));
        }

        // DELETE 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");

            if (role != 1 && role != 2)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = UserId.Value;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Notification deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}