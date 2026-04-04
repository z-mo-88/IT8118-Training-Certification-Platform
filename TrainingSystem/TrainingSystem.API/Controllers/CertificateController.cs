using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;

namespace TrainingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CertificateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CertificateController(AppDbContext context)
        {
            _context = context;
        }

        // PUBLIC endpoint (no login required)
        [AllowAnonymous]
        [HttpGet("lookup")]
        public IActionResult LookupCertificate(int userId, string reference)
        {
            var certificate = _context.Certificates
                .Include(c => c.CertificationTrack)
                .FirstOrDefault(c => c.UserId == userId &&
                                     c.CertificateReferenceNumber == reference);

            if (certificate == null)
            {
                return NotFound("Certificate not found");
            }

            return Ok(new
            {
                certificate.CertificateId,
                certificate.CertificateReferenceNumber,
                certificate.CertificateStatus,
                certificate.IssuedDate,
                TrackName = certificate.CertificationTrack.TrackName,
                UserId = certificate.UserId
            });
        }
    }
}