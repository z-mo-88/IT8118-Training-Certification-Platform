using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<CourseEquipmentRequirement> CourseEquipmentRequirements { get; set; } = new List<CourseEquipmentRequirement>();

    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
