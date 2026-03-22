using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BankSystem.Auth.Config
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {

                    
                new Client
                {
                ClientId = "bank.client",
                ClientName = "Bank Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },
                RedirectUris = { "http://localhost:3000/callback" },
                PostLogoutRedirectUris = { "http://localhost:3000" },
                AllowedCorsOrigins = { "http://localhost:3000" },
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "bank.api",
                    "roles"
                }
                },
                new Client
                {
                    ClientId = "bank.mobile",
                    ClientName = "Bank Mobile App",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    RequirePkce = true,
                    RedirectUris = { "bankapp://callback" },
                    PostLogoutRedirectUris = { "bankapp://logout" },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "bank.api",
                        "roles"
                    }
                },
                new Client
                {
                    ClientId = "bank.service",
                    ClientName = "Bank Service",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("service-secret".Sha256()) },
                    AllowedScopes = { "bank.api", "roles" }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("bank.api", "Bank API")
                {
                    Scopes = { "bank.api" },
                    UserClaims = { "role" }
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("bank.api", "Bank API Access"),
                new ApiScope("roles", "User roles")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }
    }
}