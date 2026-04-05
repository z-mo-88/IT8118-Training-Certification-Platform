using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class InstructorAvailability
{
    [Key]
    public int AvailabilityId { get; set; }

    [Required]
    [StringLength(50)]
    public string DayOfWeek { get; set; } = null!;

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    public bool IsAvailable { get; set; }

    [Required]
    public int UserId { get; set; }


    public virtual User User { get; set; } = null!;
}
