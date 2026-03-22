namespace BankSystem.Settings.DTOs
{
    public class UserSettingsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Theme { get; set; } = string.Empty;
        public List<Guid> HiddenAccountIds { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSettingsRequest
    {
        public string? Theme { get; set; }
        public List<Guid>? HiddenAccountIds { get; set; }
    }

    public class UpdateThemeRequest
    {
        public string Theme { get; set; } = string.Empty;
    }

    public class UpdateHiddenAccountsRequest
    {
        public List<Guid> AccountIds { get; set; } = new();
    }
}
