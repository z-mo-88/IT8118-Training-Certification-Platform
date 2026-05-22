using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.DTOs
{
    public class CreateCertificateDto
    {
        [Required]
        public DateOnly IssuedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string CertificateReferenceNumber { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string CertificateStatus { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CertificationTrackId { get; set; }
    }
}