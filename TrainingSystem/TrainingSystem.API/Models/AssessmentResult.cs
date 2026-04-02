using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class AssessmentResult
{
    public int ResultId { get; set; }

    public string Remarks { get; set; } = null!;

    public TimeOnly RecordTime { get; set; }

    public DateOnly RecordDate { get; set; }

    public bool IsPassed { get; set; }

    public int EnrollmentId { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
