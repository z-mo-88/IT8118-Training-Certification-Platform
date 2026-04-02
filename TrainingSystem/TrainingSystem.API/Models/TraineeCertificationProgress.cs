using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class TraineeCertificationProgress
{
    public int ProgressId { get; set; }

    public string Status { get; set; } = null!;

    public int ProgressPercent { get; set; }

    public DateOnly EligibleDate { get; set; }

    public int UserId { get; set; }

    public int CertificationTrackId { get; set; }

    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
