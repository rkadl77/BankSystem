using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BankSystem.Users.Data;
using BankSystem.Users.Models;
using BankSystem.Users.DTOs;

namespace BankSystem.Users.Services
{
    public class UserService : IUserService
    {
        private readonly UsersDbContext _context;
        private readonly HttpClient _authClient;
        private readonly ILogger<UserService> _logger;

        public UserService(UsersDbContext context, HttpClient authClient, ILogger<UserService> logger)
        {
            _context = context;
            _authClient = authClient;
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Call Auth service to create credentials
            try
            {
                var authRequest = new
                {
                    userId = user.Id,
                    email = user.Email,
                    password = request.Password
                };

                var json = JsonSerializer.Serialize(authRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Creating auth credentials for user {UserId}", user.Id);
                var response = await _authClient.PostAsync("/api/internal/users", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Auth service failed to create credentials for user {UserId}: {StatusCode} - {Error}", 
                        user.Id, response.StatusCode, errorContent);

                    // Rollback: delete user from Users DB
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    throw new InvalidOperationException($"Failed to create auth credentials: {errorContent}");
                }

                _logger.LogInformation("Auth credentials created successfully for user {UserId}", user.Id);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Exception calling Auth service for user {UserId}", user.Id);

                // Rollback: delete user from Users DB
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                throw new InvalidOperationException("Failed to create auth credentials", ex);
            }

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}