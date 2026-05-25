namespace TrainingSystem.API.DTOs
{
    public class CertificateDto
    {
        public int CertificateId { get; set; }
        public DateOnly IssuedDate { get; set; }
        public string CertificateReferenceNumber { get; set; } = null!;
        public string CertificateStatus { get; set; } = null!;
        public int UserId { get; set; }
        public int CertificationTrackId { get; set; }
        public string? TraineeName { get; set; }
        public string? TrackName { get; set; }
    }
}