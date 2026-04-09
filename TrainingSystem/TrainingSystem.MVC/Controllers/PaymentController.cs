using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                .ToListAsync();

            return View(payments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            LoadEnrollments();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (payment.AmountPaid <= 0)
                ModelState.AddModelError("", "Amount must be greater than 0");

            var enrollment = await _context.Enrollments.FindAsync(payment.EnrollmentId);

            if (enrollment == null)
                ModelState.AddModelError("", "Invalid enrollment");

            if (ModelState.IsValid)
            {
                payment.PaidDate = DateOnly.FromDateTime(DateTime.Now);

                if (payment.AmountPaid >= enrollment.OutstandingBalance)
                {
                    payment.PaymentStatus = "Paid";
                }
                else
                {
                    payment.PaymentStatus = "Partial";
                }

                _context.Payments.Add(payment);

                enrollment.OutstandingBalance -= payment.AmountPaid;

                if (enrollment.OutstandingBalance <= 0)
                {
                    enrollment.OutstandingBalance = 0;
                    enrollment.IsOverdue = false;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadEnrollments();
            return View(payment);
        }

        private void LoadEnrollments()
        {
            ViewBag.Enrollments = new SelectList(
                _context.Enrollments
                    .Include(e => e.User)
                    .Include(e => e.Session)
                        .ThenInclude(s => s.Course)
                    .Select(e => new
                    {
                        e.EnrollmentId,
                        Display = e.User.Name + " - " + e.Session.Course.Title
                    }),
                "EnrollmentId",
                "Display"
            );
        }
    }
}