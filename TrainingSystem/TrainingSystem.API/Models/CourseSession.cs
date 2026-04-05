using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class CourseSession
{
    [Key]
    public int SessionId { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Required]
    public int SessionCapacity { get; set; }

    [Required]
    public DateOnly SessionDate { get; set; }

    [Required]
    public int AvailableSeats { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int UserId { get; set; }   

    [Required]
    public int RoomId { get; set; }


    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
