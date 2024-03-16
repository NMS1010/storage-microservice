﻿using BuildingBlocks.Web;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Jwt
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJwt(this IServiceCollection services)
        {
            var jwtOptions = services.GetOptions<JwtBearerOptions>("Jwt");
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie(options => options.SlidingExpiration = true)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = jwtOptions.Authority;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.FromSeconds(2)
                    };
                    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;
                    options.MetadataAddress = jwtOptions.MetadataAddress;
                });

            if (!string.IsNullOrEmpty(jwtOptions.Audience))
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(nameof(ApiScope), policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("scope", jwtOptions.Audience);
                    });
                });
            }

            return services;
        }
    }
}
