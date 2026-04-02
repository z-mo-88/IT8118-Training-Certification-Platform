using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class InstructorProfile
{
    public int UserId { get; set; }

    public string Bio { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
