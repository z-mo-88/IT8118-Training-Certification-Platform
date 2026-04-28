namespace TrainingSystem.Reporting.Models
{
    public class EnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public string TraineeName { get; set; }
        public string CourseTitle { get; set; }
        public string Status { get; set; }
        public string EnrollmentDate { get; set; }
        public decimal OutstandingBalance { get; set; }
    }
}