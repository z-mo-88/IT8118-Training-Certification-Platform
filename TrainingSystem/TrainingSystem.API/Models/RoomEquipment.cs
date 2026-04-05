using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class RoomEquipment
{
    [Key]
    public int RoomEquipmentId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int EquipmentId { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
