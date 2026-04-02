using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class ExpertiseArea
{
    public int ExpertiseId { get; set; }

    public string ExpertiseName { get; set; } = null!;

    public virtual ICollection<InstructorExpertise> InstructorExpertises { get; set; } = new List<InstructorExpertise>();
}
