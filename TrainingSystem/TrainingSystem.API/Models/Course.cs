using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int DurationHours { get; set; }

    public int DefaultCapacity { get; set; }

    public decimal EnrollmentFee { get; set; }

    public int CategoryId { get; set; }

    public int? PrerequisiteCourseId { get; set; }

    public virtual SubjectCategory Category { get; set; } = null!;

    public virtual ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();

    public virtual ICollection<CourseEquipmentRequirement> CourseEquipmentRequirements { get; set; } = new List<CourseEquipmentRequirement>();

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<Course> InversePrerequisiteCourse { get; set; } = new List<Course>();

    public virtual Course? PrerequisiteCourse { get; set; }
}
