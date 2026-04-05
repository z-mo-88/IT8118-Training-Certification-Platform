using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    [StringLength(50)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [Required]
    public int DurationHours { get; set; }

    [Required]
    public int DefaultCapacity { get; set; }

    [Required]
    [Range(0, 99999999.99)]
    public decimal EnrollmentFee { get; set; }

    [Required]
    public int CategoryId { get; set; }


    public int? PrerequisiteCourseId { get; set; }

    public virtual SubjectCategory Category { get; set; } = null!;

    public virtual ICollection<CertificationTrackCourse> CertificationTrackCourses { get; set; } = new List<CertificationTrackCourse>();

    public virtual ICollection<CourseEquipmentRequirement> CourseEquipmentRequirements { get; set; } = new List<CourseEquipmentRequirement>();

    public virtual ICollection<CourseSession> CourseSessions { get; set; } = new List<CourseSession>();

    public virtual ICollection<Course> InversePrerequisiteCourse { get; set; } = new List<Course>();

    public virtual Course? PrerequisiteCourse { get; set; }
}
