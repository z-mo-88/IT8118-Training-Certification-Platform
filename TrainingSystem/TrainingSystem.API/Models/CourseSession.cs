using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class CourseSession
{
    public int SessionId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;

    public int SessionCapacity { get; set; }

    public DateOnly SessionDate { get; set; }

    public int AvailableSeats { get; set; }

    public int CourseId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
