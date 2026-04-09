using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> UserExistsAsync(Guid id);
        Task<UserDto> GetUserByEmailAsync(string email);
    }
}