namespace TrainingSystem.API.DTOs
{
    public class PatchUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CPR { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}