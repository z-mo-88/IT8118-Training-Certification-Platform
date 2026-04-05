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
            var certificate = await _context.Certificates
                .Include(c => c.CertificationTrack)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.CertificateReferenceNumber == reference);

            if (certificate == null)
            {
                return NotFound(new { message = "Certificate not found" });
            }

            return Ok(new
            {
                certificate.CertificateId,
                certificate.CertificateReferenceNumber,
                certificate.CertificateStatus,
                certificate.IssuedDate,
                TrackName = certificate.CertificationTrack.TrackName,
                certificate.UserId
            });
        }
    }
}