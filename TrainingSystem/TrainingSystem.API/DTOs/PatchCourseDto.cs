namespace TrainingSystem.API.DTOs
{
    public class PatchCourseDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? DurationHours { get; set; }
        public int? DefaultCapacity { get; set; }
        public decimal? EnrollmentFee { get; set; }
        public int? CategoryId { get; set; }
        public int? PrerequisiteCourseId { get; set; }
    }
}