using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string Message { get; set; } = null!;

    public DateOnly CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
