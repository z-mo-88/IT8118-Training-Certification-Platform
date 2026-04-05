using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentStatus { get; set; } = null!;

    [Required]
    [Range(0, 99999999.99)]
    public decimal AmountPaid { get; set; }

    [Required]
    public DateOnly PaidDate { get; set; }

    [Required]
    public int EnrollmentId { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
}
