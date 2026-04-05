using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [StringLength(50)]
    public string RoomName { get; set; } = null!;

    [Required]
    public int Capacity { get; set; }

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = null!;

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}
