using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingSystem.API.Models;

public partial class CourseSession
{
    [Key]
    public int SessionId { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Scheduled";

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    public int SessionCapacity { get; set; }

    [Required]
    public DateOnly SessionDate { get; set; }

    public int AvailableSeats { get; set; }

    [Required(ErrorMessage = "The Course field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The Course field is required.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "The User field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The User field is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "The Room field is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "The Room field is required.")]
    public int RoomId { get; set; }

    [ValidateNever]
    public virtual Course Course { get; set; } = null!;

    [ValidateNever]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    [ValidateNever]
    public virtual Room Room { get; set; } = null!;

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}