using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.DTOs
{
    public class CreateEnrollmentDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int SessionId { get; set; }
    }
}