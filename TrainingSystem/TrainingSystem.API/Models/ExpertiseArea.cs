using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class ExpertiseArea
{
    [Key]
    public int ExpertiseId { get; set; }

    [Required]
    [StringLength(50)]
    public string ExpertiseName { get; set; } = null!;

    public virtual ICollection<InstructorExpertise> InstructorExpertises { get; set; } = new List<InstructorExpertise>();
}
