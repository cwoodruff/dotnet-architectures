using Chinook.Maui.App.Services;
using Chinook.Maui.App.ViewModels;
using Chinook.Maui.App.Views;
using Microsoft.Extensions.Logging;

namespace Chinook.Maui.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // A native app is installed on devices you do not control, so it must never hold a SQL connection
        // string. Every piece of data comes from Chinook.Maui.Api over HTTP.
        builder.Services.AddHttpClient<ArtistsApiClient>(client => client.BaseAddress = ApiBaseAddress());

        builder.Services.AddTransient<ArtistsViewModel>();
        builder.Services.AddTransient<ArtistsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    // The Android emulator reaches the host machine at 10.0.2.2; iOS simulators and Mac Catalyst use localhost.
    private static Uri ApiBaseAddress() =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? new Uri("http://10.0.2.2:5089/")
            : new Uri("http://localhost:5089/");
}
