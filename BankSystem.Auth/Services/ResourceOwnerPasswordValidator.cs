using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using BankSystem.Auth.Services;

namespace BankSystem.Auth.Services
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IAuthService _authService;

        public ResourceOwnerPasswordValidator(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = await _authService.ValidateCredentialsAsync(context.UserName, context.Password);

            Console.WriteLine($"Validation result: Success={result.Success}, Role={result.Role}, UserId={result.UserId}");

            if (result.Success && result.UserId.HasValue)
            {
                var claims = new List<Claim>
                {
                    new Claim("role", result.Role ?? "client")
                };

                context.Result = new GrantValidationResult(
                    subject: result.UserId.Value.ToString(),
                    authenticationMethod: "password",
                    claims: claims);
            }
            else
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    result.ErrorMessage ?? "Invalid username or password");
            }
        }
    }
}