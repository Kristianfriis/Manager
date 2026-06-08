using FluentResults;
using Manager.Api.Models;
using Manager.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Manager.Api.Data;

public class PlayerService(GameDbContext context)
{
    public async Task<Result<PlayerDto>> GetPlayerAsync(string name)
    {
        var entity = await context.Players.FirstOrDefaultAsync(p => p.Name == name);
        return entity == null ? Result.Fail("Player not found") : Result.Ok(new PlayerDto { Id = entity.Id, Name = entity.Name, ShipCount = entity.ShipCount, Metal = entity.Metal });
    }

    public async Task<Result<PlayerDto>> CreatePlayerAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Name cannot be empty");

        var existing = await context.Players.AnyAsync(p => p.Name == name);
        if (existing)
            return Result.Fail("Player with this name already exists");

        var player = new PlayerEntity { Name = name, ShipCount = 10, Metal = 200 };
        context.Players.Add(player);
        await context.SaveChangesAsync();

        return Result.Ok(new PlayerDto { Id = player.Id, Name = player.Name, ShipCount = player.ShipCount, Metal = player.Metal });
    }
}