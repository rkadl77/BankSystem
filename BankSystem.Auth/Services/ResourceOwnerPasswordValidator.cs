using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using System.Threading.Tasks;

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
            var result = await _authService.ValidateCredentialsAsync(
                context.UserName,
                context.Password);

            if (result.Success && result.UserId.HasValue)
            {
                context.Result = new GrantValidationResult(
                    result.UserId.Value.ToString(),
                    "password",
                    claims: null);
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