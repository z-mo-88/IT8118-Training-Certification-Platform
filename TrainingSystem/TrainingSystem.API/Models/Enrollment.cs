using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Required]
    public DateOnly EnrollmentDate { get; set; }

    [Required]
    [Range(0, 99999999.99)]
    public decimal OutstandingBalance { get; set; }

    [Required]
    public bool IsOverdue { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int SessionId { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual CourseSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
