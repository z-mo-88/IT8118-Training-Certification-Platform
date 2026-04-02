using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class InstructorExpertise
{
    public int InstructorExpertiseId { get; set; }

    public int ExpertiseId { get; set; }

    public int UserId { get; set; }

    public virtual ExpertiseArea Expertise { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
