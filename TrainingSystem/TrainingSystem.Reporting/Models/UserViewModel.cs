namespace TrainingSystem.Reporting.Models
{
    public class UserViewModel
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
