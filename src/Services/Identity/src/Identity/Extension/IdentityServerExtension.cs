using BuildingBlocks.Web;
using Identity.Configurations;
using Identity.Data;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extension
{
    public static class IdentityServerExtension
    {
        public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
        {
            var authOptions = builder.Services.GetOptions<AuthOptions>(nameof(AuthOptions));

            builder.Services
                .AddIdentity<AppUser, IdentityRole>(config =>
                {
                    config.Password.RequiredLength = 1;
                    config.Password.RequireDigit = false;
                    config.Password.RequireNonAlphanumeric = false;
                    config.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = authOptions.IssuerUri;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<AppUser>()
                .AddResourceOwnerValidator<UserValidator>()
                .AddDeveloperSigningCredential();

            return builder;
        }
    }
}
