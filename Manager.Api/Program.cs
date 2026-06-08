using Microsoft.EntityFrameworkCore;
using Manager.Api.Data;
using Manager.Shared.Dtos;
using Manager.Api.Data.Errors;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("*")
     .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<GameDbContext>(o =>
    o.UseSqlite("Data Source=manager.db"));

builder.Services.AddScoped<GameService>();
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<GameDbContext>().Database.EnsureCreated();

app.UseCors();
app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.MapGet("/api/planets", async (GameService service) =>
{
    var planets = await service.ListPlanetsAsync();
    return Results.Ok(planets);
});

app.MapGet("/api/planets/{id:int}", async (int id, GameService service) =>
{
    var planet = await service.GetPlanetAsync(id);
    return planet is null ? Results.NotFound() : Results.Ok(planet.Value);
});

app.MapPost("/api/planets", async (CreatePlanetRequest req, GameService service) =>
{
    var planetResult = await service.CreatePlanetAsync(req.Name);
    return planetResult.IsSuccess
        ? Results.Created($"/api/planets/{planetResult.Value.Id}", planetResult.Value)
        : Results.BadRequest();
});

app.MapPost("/api/planets/{id:int}/collect", async (int id, GameService service) =>
{
    var planetResult = await service.CollectAsync(id);
    return planetResult.IsSuccess
        ? Results.Ok(planetResult.Value)
        : Results.NotFound();
});

app.MapPost("/api/planets/{id:int}/upgrade", async (int id, UpgradeRequest req, GameService service) =>
{
    var planetResult = await service.UpgradeAsync(id, req.ResourceType);

    if(planetResult.HasError<InsufficientFundsError>())
        return Results.BadRequest("Not enough resources to upgrade");

    return planetResult.IsSuccess
        ? Results.Ok(planetResult.Value)
        : Results.NotFound();
});

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

record CreatePlanetRequest(string Name);
record UpgradeRequest(ResourceType ResourceType);
