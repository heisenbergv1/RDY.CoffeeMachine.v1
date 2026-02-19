using CoffeeMachine.Api.Extensions;
using CoffeeMachine.Infrastructure.Extensions;

#pragma warning disable IDE0211 // Convert to 'Program.Main' style program
var builder = WebApplication.CreateBuilder(args);
#pragma warning restore IDE0211 // Convert to 'Program.Main' style program

// Add services to the container.
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger("Coffee Machine Service API", "v1");
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseAppSwagger(
    version: "v1",
    routePrefix: "swagger",
    displayName: "Coffee Machine Service");

app.Run();
