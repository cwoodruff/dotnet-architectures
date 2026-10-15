var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// The route table in appsettings.json is the migration plan. See the README.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapReverseProxy();

app.Run();
