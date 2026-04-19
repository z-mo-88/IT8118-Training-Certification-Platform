using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models;

public partial class CourseEquipmentRequirement
{
    [Key]
    public int CourseEquipmentRequirementId { get; set; }

    [Required]
    public int QuantityRequired { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public int EquipmentId { get; set; }

    public virtual Course? Course { get; set; }
    public virtual Equipment? Equipment { get; set; }
}