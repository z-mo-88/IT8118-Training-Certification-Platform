using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [StringLength(255)]
    public string Message { get; set; } = null!;

    [Required]
    public DateOnly CreatedAt { get; set; }

    [Required]
    public bool IsRead { get; set; }

    [Required]
    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
