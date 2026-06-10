using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Manager.Client;
using Manager.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddServiceDiscovery();

builder.Services.AddHttpClient("ManagerApi", (sp, options) =>
    {
        var endpoint = builder.Configuration.GetValue<string>("ApiBaseUrl");
        options.BaseAddress = new Uri(endpoint?? throw new InvalidOperationException("ApiBaseUrl is not configured"));
    })
    .AddServiceDiscovery()
    .AddStandardResilienceHandler();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<LocalStorageService>();

builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();
