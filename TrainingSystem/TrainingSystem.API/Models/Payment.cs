using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public decimal AmountPaid { get; set; }

    public DateOnly PaidDate { get; set; }

    public int EnrollmentId { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
