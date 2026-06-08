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
builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    ctx.Database.EnsureCreated();
    SectorService.Seed(ctx);
}

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

app.MapGet("/api/sectors", async (SectorService service) =>
{
    var sectors = await service.ListSectorsAsync();
    return Results.Ok(sectors);
});

app.MapGet("/api/sectors/{id:int}", async (int id, SectorService service) =>
{
    var sector = await service.GetSectorAsync(id);
    return sector.IsSuccess ? Results.Ok(sector.Value) : Results.NotFound();
});

app.MapPost("/api/zones/{id:int}/claim", async (int id, ClaimRequest req, SectorService service) =>
{
    var result = await service.ClaimZoneAsync(id, req.PlayerName);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
});

app.MapPost("/api/zones/{id:int}/attack", async (int id, AttackRequest req, SectorService service) =>
{
    var result = await service.AttackZoneAsync(id, req.PlayerName, req.ShipCount);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
});

app.MapGet("/api/players/{name}", async (string name, PlayerService service) =>
{
    var player = await service.GetPlayerAsync(name);
    return player.IsSuccess ? Results.Ok(player.Value) : Results.NotFound();
});

app.MapPost("/api/players/{name}", async (string name, PlayerService service) =>
{
    var player = await service.CreatePlayerAsync(name);
    return player.IsSuccess ? Results.Ok(player.Value) : Results.BadRequest(player.Errors[0].Message);
});

app.MapPost("/api/players/{name}/build-ships", async (string name, BuildShipsRequest req, SectorService service) =>
{
    var result = await service.BuildShipsAsync(name, req.Count);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors.First().Message);
});

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

record CreatePlanetRequest(string Name);
record UpgradeRequest(ResourceType ResourceType);
record ClaimRequest(string PlayerName);
record AttackRequest(string PlayerName, int ShipCount);
record BuildShipsRequest(int Count);
