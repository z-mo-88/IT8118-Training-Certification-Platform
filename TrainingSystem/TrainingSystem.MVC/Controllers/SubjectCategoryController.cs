using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class SubjectCategoryController : BaseController
    {
        private readonly AppDbContext _context;

        public SubjectCategoryController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var categories = await _context.SubjectCategories.ToListAsync();
            return View(categories);
        }

        //  CREATE 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectCategory category)
        {
            if (!ModelState.IsValid)
                return View(category);

            
            if (_context.SubjectCategories.Any(c => c.CategoryName == category.CategoryName))
            {
                ModelState.AddModelError("", "Category already exists.");
                return View(category);
            }

            _context.SubjectCategories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT 
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.SubjectCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectCategory category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _context.SubjectCategories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.SubjectCategories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int CategoryId)
        {
            var category = await _context.SubjectCategories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.CategoryId == CategoryId);

            if (category == null)
                return NotFound();

           
            if (category.Courses.Any())
            {
                TempData["Error"] = "Cannot delete category used in courses.";
                return RedirectToAction(nameof(Index));
            }

            _context.SubjectCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}