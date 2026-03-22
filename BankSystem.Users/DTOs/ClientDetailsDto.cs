using System.Collections.Generic;

namespace BankSystem.Users.DTOs
{
    public class ClientDetailsDto
    {
        public UserDto User { get; set; } = new();
        public List<AccountDto> Accounts { get; set; } = new();
        public List<CreditDto> Credits { get; set; } = new();
    }
}