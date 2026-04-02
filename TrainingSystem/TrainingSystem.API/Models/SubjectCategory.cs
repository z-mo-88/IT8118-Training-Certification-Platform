using System;
using System.Collections.Generic;

namespace TrainingSystem.API.Models;

public partial class SubjectCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
