namespace TrainingSystem.Reporting.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }

        public int RoleId { get; set; }
    }
}