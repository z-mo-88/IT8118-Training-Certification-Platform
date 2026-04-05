using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class AssessmentResult
{
    [Key]
    public int ResultId { get; set; }

    [Required]
    [StringLength(50)]
    public string Remarks { get; set; } = null!;

    [Required]
    public TimeOnly RecordTime { get; set; }

    [Required]
    public DateOnly RecordDate { get; set; }

    [Required]
    public bool IsPassed { get; set; }

    [Required]
    public int EnrollmentId { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
