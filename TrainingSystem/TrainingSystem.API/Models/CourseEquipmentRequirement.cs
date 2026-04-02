using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class CourseEquipmentRequirement
{
    public int CourseEquipmentRequirementId { get; set; }

    public int QuantityRequired { get; set; }

    public int CourseId { get; set; }

    public int EquipmentId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Equipment Equipment { get; set; } = null!;
}
