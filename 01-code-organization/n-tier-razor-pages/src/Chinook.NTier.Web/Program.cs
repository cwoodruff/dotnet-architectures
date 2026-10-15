using Chinook.NTier.Business;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Chinook")
    ?? throw new InvalidOperationException("Connection string 'Chinook' is not configured.");

builder.Services.AddRazorPages();
builder.Services.AddNTierBusiness(connectionString);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
