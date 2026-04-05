using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class InstructorProfile
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Bio { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Notes { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
