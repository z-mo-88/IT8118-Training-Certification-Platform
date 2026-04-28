namespace TrainingSystem.Reporting.Models
{
    public class CertificateViewModel
    {
        public int CertificateId { get; set; }
        public string? CertificateReferenceNumber { get; set; }
        public string? CertificateStatus { get; set; }
        public string? IssuedDate { get; set; }
        public int UserId { get; set; }
        public string? TraineeName { get; set; }
        public string? TrackName { get; set; }
    }
}