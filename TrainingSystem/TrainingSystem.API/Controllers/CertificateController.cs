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

        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> GetCertificates()
        {
            var certificates = await _context.Certificates
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CertificationTrack)
                .Select(c => new
                {
                    c.CertificateId,
                    c.CertificateReferenceNumber,
                    c.CertificateStatus,
                    c.IssuedDate,
                    c.UserId,
                    CPR = c.User.CPR,
                    TraineeName = c.User.Name,
                    TrackName = c.CertificationTrack.TrackName
                })
                .ToListAsync();

            return Ok(certificates);
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupCertificate(string cpr, string reference)
        {
            // Validate CPR and certificate reference
            if (string.IsNullOrWhiteSpace(cpr) || string.IsNullOrWhiteSpace(reference))
            {
                return BadRequest(new
                {
                    message = "CPR and certificate reference are required."
                });
            }

            cpr = cpr.Trim();
            reference = reference.Trim();

            // Search certificate using CPR + Certificate Reference
            var certificate = await _context.Certificates
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CertificationTrack)
                    .ThenInclude(ct => ct.CertificationTrackCourses)
                        .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(c =>
                    c.User.CPR == cpr &&
                    c.CertificateReferenceNumber == reference);

            // Certificate not found
            if (certificate == null)
            {
                return NotFound(new
                {
                    message = "Certificate not found"
                });
            }

            // Get completed courses
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

            // Return certificate information
            return Ok(new
            {
                certificate.CertificateId,
                certificate.CertificateReferenceNumber,
                certificate.CertificateStatus,
                certificate.IssuedDate,

                TrackName = certificate.CertificationTrack.TrackName,

                certificate.UserId,
                CPR = certificate.User.CPR,

                TraineeName = certificate.User.Name,

                CompletedCourses = completedCourses
            });
        }
    }
}