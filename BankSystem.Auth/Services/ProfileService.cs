using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace BankSystem.Auth.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProfileService _userProfileService;

        public ProfileService(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Console.WriteLine("=== ProfileService.GetProfileDataAsync called ===");

            var sub = context.Subject.FindFirst("sub")?.Value;
            Console.WriteLine($"sub: {sub}");

            if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var userId))
            {
                Console.WriteLine($"Looking for user with id: {userId}");
                var user = await _userProfileService.GetUserProfileByIdAsync(userId);
                if (user != null)
                {
                    Console.WriteLine($"User found: {user.Email}, Role: {user.Role}");
                    context.IssuedClaims.Add(new Claim("role", user.Role));
                }
                else
                {
                    Console.WriteLine($"User not found for id: {userId}");
                }
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            Console.WriteLine("=== ProfileService.IsActiveAsync called ===");
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}