using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var payments = await _context.Payments
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.User)
                .Include(p => p.Enrollment)
                    .ThenInclude(e => e.Session)
                        .ThenInclude(s => s.Course)
                .OrderByDescending(p => p.PaidDate)
                .ToListAsync();

            return View(payments);
        }

        //CREATE GET
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadEnrollments();
            return View();
        }

        //  CREATE POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ModelState.Remove("Enrollment");
            ModelState.Remove("PaymentStatus");
            ModelState.Remove("PaidDate");

            if (payment.EnrollmentId == 0)
                ModelState.AddModelError("EnrollmentId", "Please select an enrollment.");

            if (payment.AmountPaid <= 0)
                ModelState.AddModelError("AmountPaid", "Amount must be greater than 0.");

            var enrollment = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == payment.EnrollmentId);

            if (enrollment == null)
                ModelState.AddModelError("EnrollmentId", "Invalid enrollment.");

            
            if (enrollment != null && enrollment.Status == "Dropped")
                ModelState.AddModelError("EnrollmentId", "Dropped enrollments cannot receive payments.");

            if (enrollment != null)
            {
                if (enrollment.OutstandingBalance <= 0)
                    ModelState.AddModelError("EnrollmentId", "This enrollment is already fully paid.");

                if (payment.AmountPaid > enrollment.OutstandingBalance)
                    ModelState.AddModelError("AmountPaid",
                        $"Amount cannot exceed remaining balance ({enrollment.OutstandingBalance}).");
            }

            if (!ModelState.IsValid)
            {
                LoadEnrollments(payment.EnrollmentId);
                return View(payment);
            }

            // SAVE PAYMENT
            payment.PaidDate = DateOnly.FromDateTime(DateTime.Now);

            enrollment!.OutstandingBalance -= payment.AmountPaid;

            if (enrollment.OutstandingBalance <= 0)
            {
                enrollment.OutstandingBalance = 0;
                enrollment.IsOverdue = false;
                payment.PaymentStatus = "Paid";
            }
            else
            {
                payment.PaymentStatus = "Partial";

                
                if (enrollment.Session.SessionDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    enrollment.IsOverdue = true;
                }
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // DROPDOWN 
        private void LoadEnrollments(int? selectedEnrollmentId = null)
        {
            var unpaidEnrollments = _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .Where(e =>
                    e.OutstandingBalance > 0 &&
                    e.Status != "Dropped") 
                .Select(e => new
                {
                    e.EnrollmentId,
                    Display = e.User.Name + " - " +
                              e.Session.Course.Title +
                              " (Remaining: " + e.OutstandingBalance + ")"
                })
                .ToList();

            ViewBag.Enrollments = new SelectList(
                unpaidEnrollments,
                "EnrollmentId",
                "Display",
                selectedEnrollmentId
            );
        }
    }
}