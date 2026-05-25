using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class ProvidersController : BaseController
    {
        private readonly AppDbContext _context;

        public ProvidersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var providers = await _context.Providers.ToListAsync();
            return View(providers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Provider provider)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (!ModelState.IsValid)
            {
                return View(provider);
            }

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var provider = await _context.Providers.FindAsync(id);

            if (provider == null)
                return NotFound();

            return View(provider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Provider provider)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            if (!ModelState.IsValid)
            {
                return View(provider);
            }

            var existing = await _context.Providers.FindAsync(provider.ProviderId);

            if (existing == null)
                return NotFound();

            existing.ProviderName = provider.ProviderName;
            existing.Email = provider.Email;
            existing.PhoneNumber = provider.PhoneNumber;
            existing.Address = provider.Address;
            existing.IsActive = provider.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var auth = AuthorizeRole(3);
            if (auth != null) return auth;

            var provider = await _context.Providers.FindAsync(id);

            if (provider == null)
                return NotFound();

            _context.Providers.Remove(provider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}