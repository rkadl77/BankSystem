using System.Text.Json;

namespace BankSystem.Settings.Models
{
    public class UserSettings
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Theme { get; set; } = "Light";
        public string HiddenAccountIds { get; set; } = "[]";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<Guid> GetHiddenAccountsList()
        {
            if (string.IsNullOrEmpty(HiddenAccountIds))
                return new List<Guid>();

            try
            {
                return JsonSerializer.Deserialize<List<Guid>>(HiddenAccountIds) ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public void SetHiddenAccounts(List<Guid> accountIds)
        {
            HiddenAccountIds = JsonSerializer.Serialize(accountIds);
        }
    }
}
