using Identity.Identity.Constants;
using IdentityServer4;
using IdentityServer4.Models;

namespace Identity.Configurations
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
            new(Constants.StandardScopes.Roles, ["role"])
        ];

        public static IEnumerable<ApiScope> ApiScopes =>
        [
            new(Constants.StandardScopes.IdentityApi)
        ];

        public static IEnumerable<ApiResource> ApiResources =>
        [
            new(Constants.StandardScopes.IdentityApi)
        ];

        public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    Constants.StandardScopes.IdentityApi
                },
                AccessTokenLifetime = 3600,  // authorize the client to access protected resources
                IdentityTokenLifetime = 3600 // authenticate the user
            }
        ];
    }
}
