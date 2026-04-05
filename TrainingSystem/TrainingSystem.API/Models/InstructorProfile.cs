using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class InstructorProfile
{
    [Key]
    public int InstructorExpertiseId { get; set; }

    [Required]
    public int ExpertiseId { get; set; }

    [Required]
    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
