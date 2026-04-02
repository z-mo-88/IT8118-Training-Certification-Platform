using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomName { get; set; } = null!;

    public int Capacity { get; set; }

    public string Location { get; set; } = null!;

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
