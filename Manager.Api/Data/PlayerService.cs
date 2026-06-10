using FluentResults;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Data;

public class PlayerService(GameDbContext context, GameService gameService)
{
    public async Task<Result<PlayerDto>> GetPlayerAsync(int id)
    {
        var entity = await context.Players.FirstOrDefaultAsync(p => p.Id == id);
        return entity == null ? Result.Fail("Player not found") : Result.Ok(new PlayerDto { Id = entity.Id, Name = entity.Name });
    }

    public async Task<Result<PlayerDto>> CreatePlayerAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Name cannot be empty");

        var existing = await context.Players.AnyAsync(p => p.Name == name);
        if (existing)
            return Result.Fail("Player with this name already exists");

        var player = new PlayerEntity { Name = name };
        context.Players.Add(player);
        await context.SaveChangesAsync();

        // Create a starting planet for the new player
        await gameService.CreatePlanetAsync($"Player {player.Name}'s Planet", player.Id);

        return Result.Ok(new PlayerDto { Id = player.Id, Name = player.Name });
    }
}