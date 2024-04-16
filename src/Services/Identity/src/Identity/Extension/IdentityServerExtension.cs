using BuildingBlocks.EFCore;
using BuildingBlocks.Web;
using Identity.Configurations;
using Identity.Data;
using Identity.Identity.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Extension
{
    public static class IdentityServerExtension
    {
        public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
        {
            var authOptions = builder.Services.GetOptions<AuthOptions>(nameof(AuthOptions));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var postgresOptions = builder.Services.GetOptions<PostgresOptions>("PostgresOptions");

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

            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = authOptions.IssuerUri;
                })
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b
                        => b.UseNpgsql(postgresOptions.ConnectionString);
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b
                        => b.UseNpgsql(postgresOptions.ConnectionString);
                })
                .AddAspNetIdentity<AppUser>()
                .AddResourceOwnerValidator<UserValidator>()
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();

            builder.Services.AddScoped<IProfileService, ProfileService>();

            return builder;
        }
    }
}
