namespace TrainingSystem.API.DTOs
{
    public class PatchCertificateDto
    {
        public DateOnly? IssuedDate { get; set; }
        public string? CertificateReferenceNumber { get; set; }
        public string? CertificateStatus { get; set; }
        public int? UserId { get; set; }
        public int? CertificationTrackId { get; set; }
    }
}