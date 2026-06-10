using Microsoft.EntityFrameworkCore;
using Manager.Api.Data;
using Manager.Api.Hubs;
using Manager.Shared.Dtos;
using Manager.Api.Data.Errors;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Manager.Api.Models;
using Manager.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(_ => true)
     .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddDbContext<GameDbContext>(o =>
    o.UseSqlite("Data Source=manager.db"));

builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SectorService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddSignalR();
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

app.RegisterPlanetEndpoint();
app.RegisterSectorEndpoint();
app.RegisterZoneEndpoint();
app.RegisterFleetEndpoint();
app.RegisterPlayerEndpoint();
app.RegisterDataEndpoint();

app.MapHub<MovementHub>("/hub/movements");

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();