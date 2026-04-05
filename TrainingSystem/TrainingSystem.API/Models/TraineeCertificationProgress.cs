using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class TraineeCertificationProgress
{
    [Key]
    public int ProgressId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Required]
    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    [Required]
    public DateOnly EligibleDate { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int CertificationTrackId { get; set; }

    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
