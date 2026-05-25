using System.ComponentModel.DataAnnotations;

namespace TrainingSystem.API.Models
{
    public class Provider
    {
        public int ProviderId { get; set; }

        [Required(ErrorMessage = "Provider name is required.")]
        [StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    }
}