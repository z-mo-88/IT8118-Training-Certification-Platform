using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.API.DTOs;

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
        [Authorize]
        public async Task<ActionResult<IEnumerable<CertificateDto>>> GetCertificates()
        {
            var certificates = await _context.Certificates
                .AsNoTracking()
                .Select(c => new CertificateDto
                {
                    CertificateId = c.CertificateId,
                    CertificateReferenceNumber = c.CertificateReferenceNumber,
                    CertificateStatus = c.CertificateStatus,
                    IssuedDate = c.IssuedDate,
                    UserId = c.UserId,
                    CertificationTrackId = c.CertificationTrackId,
                    TraineeName = c.User.Name,
                    TrackName = c.CertificationTrack.TrackName
                })
                .ToListAsync();

            return Ok(certificates);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CertificateDto>> GetCertificateById(int id)
        {
            var certificate = await _context.Certificates
                .AsNoTracking()
                .Where(c => c.CertificateId == id)
                .Select(c => new CertificateDto
                {
                    CertificateId = c.CertificateId,
                    CertificateReferenceNumber = c.CertificateReferenceNumber,
                    CertificateStatus = c.CertificateStatus,
                    IssuedDate = c.IssuedDate,
                    UserId = c.UserId,
                    CertificationTrackId = c.CertificationTrackId
                })
                .FirstOrDefaultAsync();

            if (certificate == null)
                return NotFound();

            return Ok(certificate);
        }

        [HttpPost]
        [Authorize(Roles = "3")]
        public async Task<ActionResult<CertificateDto>> CreateCertificate(CreateCertificateDto certificateDto)
        {
            var certificate = new Certificate
            {
                IssuedDate = certificateDto.IssuedDate,
                CertificateReferenceNumber = certificateDto.CertificateReferenceNumber,
                CertificateStatus = certificateDto.CertificateStatus,
                UserId = certificateDto.UserId,
                CertificationTrackId = certificateDto.CertificationTrackId
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            var result = new CertificateDto
            {
                CertificateId = certificate.CertificateId,
                IssuedDate = certificate.IssuedDate,
                CertificateReferenceNumber = certificate.CertificateReferenceNumber,
                CertificateStatus = certificate.CertificateStatus,
                UserId = certificate.UserId,
                CertificationTrackId = certificate.CertificationTrackId
            };

            return CreatedAtAction(nameof(GetCertificateById), new { id = certificate.CertificateId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> UpdateCertificate(int id, UpdateCertificateDto certificateDto)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
                return NotFound();

            certificate.IssuedDate = certificateDto.IssuedDate;
            certificate.CertificateReferenceNumber = certificateDto.CertificateReferenceNumber;
            certificate.CertificateStatus = certificateDto.CertificateStatus;
            certificate.UserId = certificateDto.UserId;
            certificate.CertificationTrackId = certificateDto.CertificationTrackId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> PatchCertificate(int id, PatchCertificateDto certificateDto)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
                return NotFound();

            if (certificateDto.IssuedDate.HasValue)
                certificate.IssuedDate = certificateDto.IssuedDate.Value;

            if (certificateDto.CertificateReferenceNumber != null)
                certificate.CertificateReferenceNumber = certificateDto.CertificateReferenceNumber;

            if (certificateDto.CertificateStatus != null)
                certificate.CertificateStatus = certificateDto.CertificateStatus;

            if (certificateDto.UserId.HasValue)
                certificate.UserId = certificateDto.UserId.Value;

            if (certificateDto.CertificationTrackId.HasValue)
                certificate.CertificationTrackId = certificateDto.CertificationTrackId.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);

            if (certificate == null)
                return NotFound();

            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> LookupCertificate(string cpr, string reference)
        {
            if (string.IsNullOrWhiteSpace(cpr) || string.IsNullOrWhiteSpace(reference))
            {
                return BadRequest(new
                {
                    message = "CPR and certificate reference are required."
                });
            }

            cpr = cpr.Trim();
            reference = reference.Trim();

            var certificate = await _context.Certificates
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CertificationTrack)
                    .ThenInclude(ct => ct.CertificationTrackCourses)
                        .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(c =>
                    c.User.CPR == cpr &&
                    c.CertificateReferenceNumber == reference);

            if (certificate == null)
            {
                return NotFound(new
                {
                    message = "Certificate not found"
                });
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
                CPR = certificate.User.CPR,
                TraineeName = certificate.User.Name,
                CompletedCourses = completedCourses
            });
        }
    }
}