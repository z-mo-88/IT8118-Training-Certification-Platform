using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class CertificationTrackCourse
{
    [Key]
    public int TrackCourseId { get; set; }

    [Required]
    public bool IsRequired { get; set; }

    [Required]
    public int CertificationTrackId { get; set; }

    [Required]
    public int CourseId { get; set; }


    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
}
