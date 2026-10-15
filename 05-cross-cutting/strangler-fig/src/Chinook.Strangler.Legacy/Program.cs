var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// No UseHttpsRedirection: this app sits behind the Facade. A redirect from here would send the
// browser straight to Legacy's own port and around the strangler.
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapDefaultEndpoints();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
