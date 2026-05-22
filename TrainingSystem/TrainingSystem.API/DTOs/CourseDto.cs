namespace TrainingSystem.API.DTOs
{
    public class CourseDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int DurationHours { get; set; }
        public int DefaultCapacity { get; set; }
        public decimal EnrollmentFee { get; set; }
        public int CategoryId { get; set; }
        public int? PrerequisiteCourseId { get; set; }
    }
}