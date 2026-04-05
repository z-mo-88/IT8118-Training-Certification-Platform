using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Certificate
{
    [Key]
    public int CertificateId { get; set; }

    [Required]
    public DateOnly IssuedDate { get; set; }

    [Required]
    [StringLength(50)]
    public string CertificateReferenceNumber { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string CertificateStatus { get; set; } = null!;

    [Required]
    public int UserId { get; set; }

    [Required]
    public int CertificationTrackId { get; set; }

    public virtual CertificationTrack CertificationTrack { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
