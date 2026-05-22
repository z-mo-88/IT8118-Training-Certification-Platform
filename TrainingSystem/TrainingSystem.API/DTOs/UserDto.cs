namespace TrainingSystem.API.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string CPR { get; set; } = null!;
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }
}