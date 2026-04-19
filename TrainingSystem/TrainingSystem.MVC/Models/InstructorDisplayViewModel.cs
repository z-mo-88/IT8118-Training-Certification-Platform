namespace TrainingSystem.MVC.Models
{
    public class InstructorDisplayViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        public bool IsActive { get; set; }

        public string Bio { get; set; } = "";
        public string Notes { get; set; } = "";

        public List<string> ExpertiseNames { get; set; } = new();
        public List<string> AvailabilityText { get; set; } = new();
    }
}