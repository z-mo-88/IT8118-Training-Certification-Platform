using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using TrainingSystem.API.Data;
using TrainingSystem.API.Models;
using TrainingSystem.API.Hubs;
using TrainingSystem.API.DTOs;

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

        public class RecordResultRequest
        {
            public int EnrollmentId { get; set; }
            public bool IsPassed { get; set; }
            public string? Remarks { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    Status = e.Status,
                    EnrollmentDate = e.EnrollmentDate,
                    OutstandingBalance = e.OutstandingBalance,
                    IsOverdue = e.IsOverdue,
                    UserId = e.UserId,
                    SessionId = e.SessionId,
                    TraineeName = e.User.Name,
                    CourseTitle = e.Session.Course.Title
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentDto>> GetEnrollmentById(int id)
        {
            var enrollment = await _context.Enrollments
                .Where(e => e.EnrollmentId == id)
                .Select(e => new EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    Status = e.Status,
                    EnrollmentDate = e.EnrollmentDate,
                    OutstandingBalance = e.OutstandingBalance,
                    IsOverdue = e.IsOverdue,
                    UserId = e.UserId,
                    SessionId = e.SessionId
                })
                .FirstOrDefaultAsync();

            if (enrollment == null)
                return NotFound();

            return Ok(enrollment);
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(CreateEnrollmentDto enrollmentDto)
        {
            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == enrollmentDto.SessionId);

            if (session == null)
                return BadRequest("Invalid session.");

            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == enrollmentDto.UserId && e.SessionId == enrollmentDto.SessionId);

            if (alreadyEnrolled)
                return BadRequest("User is already enrolled in this session.");

            if (session.AvailableSeats <= 0)
                return BadRequest("No available seats for this session.");

            var enrollment = new Enrollment
            {
                UserId = enrollmentDto.UserId,
                SessionId = enrollmentDto.SessionId,
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Enrolled",
                OutstandingBalance = 0,
                IsOverdue = false
            };

            session.AvailableSeats--;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await BroadcastSeatsAsync(session.SessionId);

            var result = new EnrollmentDto
            {
                EnrollmentId = enrollment.EnrollmentId,
                Status = enrollment.Status,
                EnrollmentDate = enrollment.EnrollmentDate,
                OutstandingBalance = enrollment.OutstandingBalance,
                IsOverdue = enrollment.IsOverdue,
                UserId = enrollment.UserId,
                SessionId = enrollment.SessionId
            };

            return CreatedAtAction(nameof(GetEnrollmentById), new { id = enrollment.EnrollmentId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollment(int id, UpdateEnrollmentDto updatedEnrollment)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            enrollment.Status = updatedEnrollment.Status;
            enrollment.EnrollmentDate = updatedEnrollment.EnrollmentDate;
            enrollment.OutstandingBalance = updatedEnrollment.OutstandingBalance;
            enrollment.IsOverdue = updatedEnrollment.IsOverdue;
            enrollment.UserId = updatedEnrollment.UserId;
            enrollment.SessionId = updatedEnrollment.SessionId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEnrollment(int id, PatchEnrollmentDto patchEnrollment)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            if (patchEnrollment.Status != null)
                enrollment.Status = patchEnrollment.Status;

            if (patchEnrollment.EnrollmentDate.HasValue)
                enrollment.EnrollmentDate = patchEnrollment.EnrollmentDate.Value;

            if (patchEnrollment.OutstandingBalance.HasValue)
                enrollment.OutstandingBalance = patchEnrollment.OutstandingBalance.Value;

            if (patchEnrollment.IsOverdue.HasValue)
                enrollment.IsOverdue = patchEnrollment.IsOverdue.Value;

            if (patchEnrollment.UserId.HasValue)
                enrollment.UserId = patchEnrollment.UserId.Value;

            if (patchEnrollment.SessionId.HasValue)
                enrollment.SessionId = patchEnrollment.SessionId.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();

            int sessionId = enrollment.SessionId;

            var session = await _context.CourseSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
                session.AvailableSeats++;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            await BroadcastSeatsAsync(sessionId);

            return NoContent();
        }

        [HttpPost("record-result")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RecordResult([FromBody] RecordResultRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var enrollment = await _context.Enrollments
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null)
                return NotFound("Enrollment not found.");

            if (enrollment.OutstandingBalance > 0)
                return BadRequest("Cannot complete the course until payment is completed.");

            if (enrollment.Status != "Confirmed" && enrollment.Status != "Attending")
                return BadRequest("Only confirmed or attending enrollments can be completed.");

            bool resultAlreadyExists = await _context.AssessmentResults
                .AnyAsync(a => a.EnrollmentId == request.EnrollmentId);

            if (resultAlreadyExists)
                return BadRequest("Result already recorded for this enrollment.");

            enrollment.Status = "Completed";

            var result = new AssessmentResult
            {
                EnrollmentId = request.EnrollmentId,
                IsPassed = request.IsPassed,
                Remarks = string.IsNullOrWhiteSpace(request.Remarks) ? "No remarks" : request.Remarks,
                RecordDate = DateOnly.FromDateTime(DateTime.Now),
                RecordTime = TimeOnly.FromDateTime(DateTime.Now)
            };

            _context.AssessmentResults.Add(result);

            if (request.IsPassed)
            {
                int userId = enrollment.UserId;
                int courseId = enrollment.Session.CourseId;

                var trackCourses = await _context.CertificationTrackCourses
                    .Where(tc => tc.CourseId == courseId)
                    .ToListAsync();

                foreach (var trackCourse in trackCourses)
                {
                    int trackId = trackCourse.CertificationTrackId;

                    var requiredCourseIds = await _context.CertificationTrackCourses
                        .Where(tc => tc.CertificationTrackId == trackId && tc.IsRequired)
                        .Select(tc => tc.CourseId)
                        .ToListAsync();

                    if (!requiredCourseIds.Any())
                    {
                        requiredCourseIds = await _context.CertificationTrackCourses
                            .Where(tc => tc.CertificationTrackId == trackId)
                            .Select(tc => tc.CourseId)
                            .ToListAsync();
                    }

                    var passedCourseIds = await _context.AssessmentResults
                        .Include(a => a.Enrollment)
                            .ThenInclude(e => e.Session)
                        .Where(a => a.Enrollment.UserId == userId && a.IsPassed)
                        .Select(a => a.Enrollment.Session.CourseId)
                        .Distinct()
                        .ToListAsync();

                    bool completedTrack = requiredCourseIds.All(id => passedCourseIds.Contains(id));

                    int progressPercent = requiredCourseIds.Count == 0
                        ? 0
                        : (int)Math.Round((double)passedCourseIds.Count / requiredCourseIds.Count * 100);

                    if (progressPercent > 100)
                        progressPercent = 100;

                    var progress = await _context.TraineeCertificationProgresses
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.CertificationTrackId == trackId);

                    if (progress == null)
                    {
                        progress = new TraineeCertificationProgress
                        {
                            UserId = userId,
                            CertificationTrackId = trackId,
                            Status = completedTrack ? "Eligible" : "In Progress",
                            ProgressPercent = progressPercent,
                            EligibleDate = completedTrack
                                ? DateOnly.FromDateTime(DateTime.Now)
                                : DateOnly.MinValue
                        };

                        _context.TraineeCertificationProgresses.Add(progress);
                    }
                    else
                    {
                        progress.Status = completedTrack ? "Eligible" : "In Progress";
                        progress.ProgressPercent = progressPercent;

                        if (completedTrack)
                            progress.EligibleDate = DateOnly.FromDateTime(DateTime.Now);
                    }

                    if (completedTrack)
                    {
                        bool certificateExists = await _context.Certificates
                            .AnyAsync(c => c.UserId == userId && c.CertificationTrackId == trackId);

                        if (!certificateExists)
                        {
                            var certificate = new Certificate
                            {
                                UserId = userId,
                                CertificationTrackId = trackId,
                                IssuedDate = DateOnly.FromDateTime(DateTime.Now),
                                CertificateReferenceNumber = GenerateCertificateReference(),
                                CertificateStatus = "Certified"
                            };

                            _context.Certificates.Add(certificate);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully."
            });
        }

        private async Task BroadcastSeatsAsync(int sessionId)
        {
            var session = await _context.CourseSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
            {
                await _hubContext.Clients.All.SendAsync(
                    "EnrollmentUpdated",
                    session.SessionId,
                    session.AvailableSeats
                );
            }
        }

        private string GenerateCertificateReference()
        {
            return $"CERT-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}