using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<InstructorAvailability> InstructorAvailabilities { get; set; } = new List<InstructorAvailability>();

    public virtual ICollection<InstructorExpertise> InstructorExpertises { get; set; } = new List<InstructorExpertise>();

    public virtual InstructorProfile? InstructorProfile { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TraineeCertificationProgress> TraineeCertificationProgresses { get; set; } = new List<TraineeCertificationProgress>();
}
