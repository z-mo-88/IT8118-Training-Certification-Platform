using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.DTOs
{
    public class UpdateEnrollmentDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = null!;

        [Required]
        public DateOnly EnrollmentDate { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal OutstandingBalance { get; set; }

        [Required]
        public bool IsOverdue { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SessionId { get; set; }
    }
}