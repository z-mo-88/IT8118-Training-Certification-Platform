using Microsoft.AspNetCore.Mvc;
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

        // INDEX
        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var payments = await _context.Payments
                .Include(p => p.Enrollment)
                .ThenInclude(e => e.User)
                .ToListAsync();

            return View(payments);
        }

        // CREATE 
        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            ViewBag.Enrollments = _context.Enrollments
                .Include(e => e.User)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Payment payment)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (payment.AmountPaid <= 0)
                ModelState.AddModelError("", "Amount must be greater than 0");

            if (ModelState.IsValid)
            {
                payment.PaidDate = DateOnly.FromDateTime(DateTime.Now);
                payment.PaymentStatus = "Paid";

                _context.Payments.Add(payment);

                // 🔥 Update enrollment balance
                var enrollment = await _context.Enrollments.FindAsync(payment.EnrollmentId);
                if (enrollment != null)
                {
                    enrollment.OutstandingBalance -= payment.AmountPaid;

                    if (enrollment.OutstandingBalance <= 0)
                    {
                        enrollment.OutstandingBalance = 0;
                        enrollment.IsOverdue = false;
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Enrollments = _context.Enrollments
                .Include(e => e.User)
                .ToList();

            return View(payment);
        }
    }
}