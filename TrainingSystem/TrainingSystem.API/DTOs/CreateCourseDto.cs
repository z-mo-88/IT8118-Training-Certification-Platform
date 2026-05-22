using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.DTOs
{
    public class CreateCourseDto
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public int DurationHours { get; set; }

        [Required]
        public int DefaultCapacity { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal EnrollmentFee { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? PrerequisiteCourseId { get; set; }
    }
}