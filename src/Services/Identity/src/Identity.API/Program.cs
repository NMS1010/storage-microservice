using BuildingBlocks.Consul;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddConsul();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCustomProblemDetails();

app.UseCorrelationId();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.MapGet("/api/healths", () => Results.Ok());

app.MapGet("/api/identity/test", x => x.Response.WriteAsync("Test identity"));
app.MapGet("/api/identity/exception", x => throw new Exception("Test"));

app.Run();
