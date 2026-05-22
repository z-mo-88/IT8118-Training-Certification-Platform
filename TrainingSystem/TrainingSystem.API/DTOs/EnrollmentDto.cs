namespace TrainingSystem.API.DTOs
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public string Status { get; set; } = null!;
        public DateOnly EnrollmentDate { get; set; }
        public decimal OutstandingBalance { get; set; }
        public bool IsOverdue { get; set; }
        public int UserId { get; set; }
        public int SessionId { get; set; }
    }
}
