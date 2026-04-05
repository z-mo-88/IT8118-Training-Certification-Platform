using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.API.Hubs;

namespace TrainingSystem.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<EnrollmentHub> _hubContext;

        public EnrollmentsController(AppDbContext context, IHubContext<EnrollmentHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            return await _context.Enrollments.ToListAsync();
        }


        [HttpPost]
        public async Task<ActionResult> CreateEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();


            await _hubContext.Clients.All.SendAsync("EnrollmentUpdated", enrollment.SessionId);

            return Ok(enrollment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return NotFound();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}