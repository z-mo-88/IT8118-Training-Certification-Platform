using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class CertificationTrack
{
    public int CertificationTrackId { get; set; }

    public string TrackName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();

    public virtual ICollection<TraineeCertificationProgress> TraineeCertificationProgresses { get; set; } = new List<TraineeCertificationProgress>();
}
