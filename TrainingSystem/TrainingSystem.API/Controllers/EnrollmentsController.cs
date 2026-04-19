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

        public class RecordResultRequest
        {
            public int EnrollmentId { get; set; }
            public bool IsPassed { get; set; }
            public string? Remarks { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Session)
                    .ThenInclude(s => s.Course)
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpPost]
        public async Task<ActionResult> CreateEnrollment(Enrollment enrollment)
        {
            var session = await _context.CourseSessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SessionId == enrollment.SessionId);

            if (session == null)
                return BadRequest("Invalid session.");

            bool alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == enrollment.UserId && e.SessionId == enrollment.SessionId);

            if (alreadyEnrolled)
                return BadRequest("User is already enrolled in this session.");

            if (session.AvailableSeats <= 0)
                return BadRequest("No available seats for this session.");

            if (enrollment.EnrollmentDate == default)
                enrollment.EnrollmentDate = DateOnly.FromDateTime(DateTime.Now);

            if (string.IsNullOrWhiteSpace(enrollment.Status))
                enrollment.Status = "Enrolled";

            if (enrollment.OutstandingBalance < 0)
                enrollment.OutstandingBalance = 0;

            session.AvailableSeats--;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await BroadcastSeatsAsync(session.SessionId);

            return Ok(enrollment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
                return NotFound();

            int sessionId = enrollment.SessionId;

            var session = await _context.CourseSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
            {
                session.AvailableSeats++;
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            await BroadcastSeatsAsync(sessionId);

            return NoContent();
        }

        [HttpPost("record-result")]
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
                        {
                            progress.EligibleDate = DateOnly.FromDateTime(DateTime.Now);
                        }
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