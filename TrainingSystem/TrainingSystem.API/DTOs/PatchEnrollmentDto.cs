namespace TrainingSystem.API.DTOs
{
    public class PatchEnrollmentDto
    {
        public string? Status { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public decimal? OutstandingBalance { get; set; }
        public bool? IsOverdue { get; set; }
        public int? UserId { get; set; }
        public int? SessionId { get; set; }
    }
}