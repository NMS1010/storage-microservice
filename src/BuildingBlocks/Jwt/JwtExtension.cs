﻿using BuildingBlocks.Web;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;

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
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            var problemDetail = GetProblemDetails(context.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                "Unauthorized",
                                "You need the access_token to access this resource.");

                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                            var result = JsonConvert.SerializeObject(problemDetail);
                            context.Response.ContentType = "application/json+problem";
                            await context.Response.WriteAsync(result);
                        },
                        OnForbidden = async context =>
                        {
                            var problemDetail = GetProblemDetails(context.HttpContext,
                                StatusCodes.Status403Forbidden,
                                "Forbidden",
                                "You are not allowed to access this resource.");

                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                            var result = JsonConvert.SerializeObject(problemDetail);
                            context.Response.ContentType = "application/json+problem";
                            await context.Response.WriteAsync(result);
                        }
                    };
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

        private static Microsoft.AspNetCore.Mvc.ProblemDetails GetProblemDetails(HttpContext context, int statusCode, string title, string detail)
        {
            return new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Title = title,
                Extensions =
                {
                    ["TraceId"] = context.TraceIdentifier,
                    ["CorrelationId"] = context.GetCorrelationId().ToString()
                },
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };
        }
    }
}
