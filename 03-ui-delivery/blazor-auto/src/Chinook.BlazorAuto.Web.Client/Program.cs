using Chinook.BlazorAuto.Web.Client.Artists;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// In the browser the page reads through GET /api/artists on the Web project, which is the same origin.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IArtistsClient, HttpArtistsClient>();

await builder.Build().RunAsync();
