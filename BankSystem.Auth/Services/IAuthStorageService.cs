using System;
using System.Threading.Tasks;

namespace BankSystem.Auth.Services
{
    public interface IAuthStorageService
    {
        Task CreateAuthUserAsync(Guid userId, string email, string password);
    }
}