using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CertificateController(AppDbContext context)
        {
            _context = context;
        }

        // PUBLIC endpoint (no login required)
        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupCertificate(int userId, string reference)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(reference))
            {
                return BadRequest(new { message = "Trainee ID and certificate reference are required." });
            }

            reference = reference.Trim();

            var certificate = await _context.Certificates
                .AsNoTracking()
                .Include(c => c.CertificationTrack)
                    .ThenInclude(ct => ct.CertificationTrackCourses)
                        .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.CertificateReferenceNumber == reference);

            if (certificate == null)
            {
                return NotFound(new { message = "Certificate not found" });
            }

            var completedCourses = certificate.CertificationTrack.CertificationTrackCourses
                .Where(tc => tc.Course != null)
                .Select(tc => new
                {
                    tc.CourseId,
                    tc.Course.Title,
                    tc.IsRequired
                })
                .OrderBy(c => c.Title)
                .ToList();

            return Ok(new
            {
                certificate.CertificateId,
                certificate.CertificateReferenceNumber,
                certificate.CertificateStatus,
                certificate.IssuedDate,
                TrackName = certificate.CertificationTrack.TrackName,
                certificate.UserId,
                CompletedCourses = completedCourses
            });
        }
    }
}