using System;

namespace BankSystem.Users.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}