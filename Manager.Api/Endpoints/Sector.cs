using Manager.Api.Data;

namespace Manager.Api.Endpoints;

public static class SectorEndpoints
{
    public static void RegisterSectorEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/sectors").WithTags("Sectors");

        group.MapGet("/", async (SectorService service) =>
        {
            var sectors = await service.ListSectorsAsync();
            return Results.Ok(sectors);
        });

        group.MapGet("/{id:int}", async (int id, SectorService service) =>
        {
            var sector = await service.GetSectorAsync(id);
            return sector.IsSuccess ? Results.Ok(sector.Value) : Results.NotFound();
        });
    }
}