using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(9)]
        public string CPR { get; set; } = null!;

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}