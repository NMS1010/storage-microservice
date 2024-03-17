using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Web;
using Identity.Configurations;
using Identity.Extension;
using Identity.Identity.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    // Service provider validation
    // ref: https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment()
        || context.HostingEnvironment.IsStaging()
        || context.HostingEnvironment.IsEnvironment("tests");
    options.ValidateOnBuild = true;
});

builder.Services.AddMinimalEndpoints(assemblies: typeof(IdentityRoot).Assembly);
builder.AddInfrastructure();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseInfrastructure();
app.MapMinimalEndpoints();

app.MapGet("/api/identity/test", x => x.Response.WriteAsJsonAsync(new APIResponse<string>(200, "Test identity"))).RequireAuthorization(x =>
{
    x.RequireAuthenticatedUser();
    x.RequireRole(Constants.Role.USER);
    x.AuthenticationSchemes = [JwtBearerDefaults.AuthenticationScheme];
});

app.MapGet("/api/identity/exception", x => throw new Exception("Test"));

app.Run();
