using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;

namespace TrainingSystem.MVC.Controllers
{
    public class CertificationTracksController : Controller
    {
        private readonly AppDbContext _context;

        public CertificationTracksController(AppDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public async Task<IActionResult> Index()
        {
            var tracks = await _context.CertificationTracks.ToListAsync();
            return View(tracks);
        }

        // ================= CREATE =================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CertificationTrack track)
        {
            if (!ModelState.IsValid)
                return View(track);

            _context.CertificationTracks.Add(track);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================
        public async Task<IActionResult> Edit(int id)
        {
            var track = await _context.CertificationTracks.FindAsync(id);
            if (track == null)
                return NotFound();

            return View(track);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CertificationTrack track)
        {
            if (id != track.CertificationTrackId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(track);

            _context.Update(track);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ================= DELETE =================
        public async Task<IActionResult> Delete(int id)
        {
            var track = await _context.CertificationTracks
                .FirstOrDefaultAsync(t => t.CertificationTrackId == id);

            if (track == null)
                return NotFound();

            return View(track);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var track = await _context.CertificationTracks
                .Include(t => t.CertificationTrackCourses)
                .FirstOrDefaultAsync(t => t.CertificationTrackId == id);

            if (track == null)
                return NotFound();

            // delete related courses first
            _context.CertificationTrackCourses.RemoveRange(track.CertificationTrackCourses);

            _context.CertificationTracks.Remove(track);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}