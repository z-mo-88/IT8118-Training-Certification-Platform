using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class InstructorAvailability
{
    public int AvailabilityId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
