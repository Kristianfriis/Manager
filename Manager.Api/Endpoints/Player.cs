using Manager.Api.Data;
using Manager.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Endpoints;

public static class PlayerEndpoints
{
    public static void RegisterPlayerEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/players").WithTags("Players");

        group.MapGet("/{id:int}", async (int id, PlayerService service) =>
        {
            var player = await service.GetPlayerAsync(id);
            return player.IsSuccess ? Results.Ok(player.Value) : Results.NotFound();
        });

        group.MapGet("/", async (GameDbContext context) =>
        {
            var players = await context.Players.Select(p => new PlayerDto { Id = p.Id, Name = p.Name }).ToListAsync();
            return Results.Ok(players);
        });

        group.MapPost("/", async ([FromBody] CreatePlayerRequest req, PlayerService service) =>
        {
            var player = await service.CreatePlayerAsync(req.Name);
            return player.IsSuccess ? Results.Ok(player.Value) : Results.BadRequest(player.Errors[0].Message);
        });
    }
}