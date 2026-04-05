using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class SubjectCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(50)]
    public string CategoryName { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
