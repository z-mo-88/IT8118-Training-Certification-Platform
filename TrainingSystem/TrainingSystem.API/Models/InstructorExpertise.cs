using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class InstructorExpertise
{
    [Key]
    public int InstructorExpertiseId { get; set; }

    [Required]
    public int ExpertiseId { get; set; }

    [Required]
    public int UserId { get; set; }

    public virtual ExpertiseArea Expertise { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
