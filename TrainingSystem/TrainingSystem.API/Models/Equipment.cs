using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Equipment
{
    [Key]
    public int EquipmentId { get; set; }

    [Required]
    [StringLength(50)]
    public string EquipmentName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    public int? ProviderId { get; set; }

    public virtual Provider? Provider { get; set; }

    public virtual ICollection<CourseEquipmentRequirement> CourseEquipmentRequirements { get; set; } = new List<CourseEquipmentRequirement>();

    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
