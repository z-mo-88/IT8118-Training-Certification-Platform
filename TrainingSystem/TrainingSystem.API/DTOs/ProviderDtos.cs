namespace TrainingSystem.API.DTOs
{
    public class CreateProviderDto
    {
        public string ProviderName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProviderDto
    {
        public string ProviderName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProviderDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}