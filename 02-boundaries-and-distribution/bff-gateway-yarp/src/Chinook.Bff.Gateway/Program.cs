using Chinook.Bff.Gateway.Bff;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Route table and clusters live in appsettings.json under ReverseProxy. Cluster destinations use
// service names (https+http://catalog-api), which Aspire service discovery resolves at runtime.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

// AddServiceDefaults wires service discovery into every HttpClient, so these names resolve too.
builder.Services.AddHttpClient<CatalogClient>(client => client.BaseAddress = new Uri("https+http://catalog-api"));
builder.Services.AddHttpClient<CustomersClient>(client => client.BaseAddress = new Uri("https+http://customers-api"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapBffHome();
app.MapReverseProxy();

app.Run();
