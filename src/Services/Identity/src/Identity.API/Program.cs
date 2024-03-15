using BuildingBlocks.Core.CustomAPIResponse;
using BuildingBlocks.Web;
using Identity.Configurations;
using Identity.Extension;

var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

builder.Host.UseDefaultServiceProvider((context, options) =>
{
    // Service provider validation
    // ref: https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    options.ValidateScopes = context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsStaging() || context.HostingEnvironment.IsEnvironment("tests");
    options.ValidateOnBuild = true;
});

builder.Services.AddMinimalEndpoints(assemblies: typeof(IdentityRoot).Assembly);
builder.AddInfrastructure();

app.MapMinimalEndpoints();
app.UseAuthentication();
app.UseAuthorization();
app.UseInfrastructure();

app.MapGet("/api/identity/test", x => x.Response.WriteAsJsonAsync(new APIResponse<string>(200, "Test identity")));

app.MapGet("/api/identity/exception", x => throw new Exception("Test"));

app.Run();
