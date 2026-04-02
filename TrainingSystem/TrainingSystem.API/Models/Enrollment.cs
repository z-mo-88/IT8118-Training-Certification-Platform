using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly EnrollmentDate { get; set; }

    public decimal OutstandingBalance { get; set; }

    public bool IsOverdue { get; set; }

    public int UserId { get; set; }

    public int SessionId { get; set; }

    public virtual ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual CourseSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
