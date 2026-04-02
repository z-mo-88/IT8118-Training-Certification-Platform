using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class CertificationTrackCourse
{
    public int TrackCourseId { get; set; }

    public bool IsRequired { get; set; }

    public int CertificationTrackId { get; set; }

    public int CourseId { get; set; }

    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
}
