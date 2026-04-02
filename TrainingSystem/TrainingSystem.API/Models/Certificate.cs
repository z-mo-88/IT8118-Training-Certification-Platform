using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public DateOnly IssuedDate { get; set; }

    public string CertificateReferenceNumber { get; set; } = null!;

    public string CertificateStatus { get; set; } = null!;

    public int UserId { get; set; }

    public int CertificationTrackId { get; set; }

    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
