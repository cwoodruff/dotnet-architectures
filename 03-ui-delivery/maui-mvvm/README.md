# .NET MAUI with MVVM

## What this is

A native .NET MAUI app for Android, iOS, Mac Catalyst and Windows, built with the Model-View-ViewModel pattern using CommunityToolkit.Mvvm. The view is XAML bound to a view model, and the view model holds state and commands with no reference to any view type. A native client is installed on devices I do not control, so it must never hold a SQL connection string. For that reason the example also carries `Chinook.Maui.Api`, a minimal API that is the app's only path to the data.

## When I would pick it

When the app has to be a real installed app: offline use, device features like the camera, notifications or the file system, app store distribution, or a UI that should feel native on each platform. MVVM pays off here because the view models are plain C# that I can unit test without a device or an emulator. One codebase covers four platforms.

## When I would not

When a web app would do. Native distribution brings app store reviews, signing, platform SDKs and a slower release cycle, and the team still has to build and run the API. If users only need forms and lists in a browser, Razor Pages with htmx is a fraction of the work. I would also hesitate if the team has no one willing to own per-platform quirks.

## What to look at first

Open `src/Chinook.Maui.App/ViewModels/ArtistsViewModel.cs`. It is an `ObservableObject` with an `ObservableCollection<ArtistItem>`, and the `[RelayCommand]` on `LoadArtistsAsync` generates `LoadArtistsCommand`. Then open `src/Chinook.Maui.App/Views/ArtistsPage.xaml`: its button binds to that command and its `CollectionView` binds to the collection, both with compiled bindings (`x:DataType`). The view model calls `Services/ArtistsApiClient.cs`, which calls `GET /artists` on `src/Chinook.Maui.Api/Program.cs`. `MauiProgram.cs` points the client at `http://10.0.2.2:5089/` on the Android emulator and at `http://localhost:5089/` everywhere else.

**The App project was written by hand and is not built by `build-all`.** The .NET MAUI workload could not be installed where this example was scaffolded: `dotnet workload install maui` needed elevated permissions, and Xcode was not installed. So `dotnet new maui` was unavailable. `src/Chinook.Maui.App/` follows the template's shape (single project, `UseMaui`, `Platforms/` bootstrap files) minus the icon, splash and font resources. It carries a `.skip-build` marker, and `build-all.sh` / `build-all.ps1` then build the Api and the test project individually and skip the App. Package versions in the App's `.csproj` came from `dotnet package search` because `dotnet add package` cannot restore a `UseMaui` project without the workload. To build the App, run `dotnet workload install maui`, then `dotnet build src/Chinook.Maui.App -f net10.0-android` (or `net10.0-maccatalyst` with Xcode), and remove the marker once it builds. The test project deliberately does not reference the App, so the tests run without the workload.

## Data access

EF Core with `Microsoft.EntityFrameworkCore.SqlServer`, in `Chinook.Maui.Api` only; the App has no data access of its own and talks HTTP. The context and entities in `src/Chinook.Maui.Api/Data/` were scaffolded from the live database with `dotnet ef dbcontext scaffold --no-onconfiguring`; `dotnet-ef` is a local tool (`dotnet tool restore`).

The Api connects to one database: `Chinook`, via `ConnectionStrings:Chinook` in `src/Chinook.Maui.Api/appsettings.Development.json`. Start it with `database/start-db.sh` and `database/init-db.sh`, then `dotnet run --project src/Chinook.Maui.Api` (port 5089) before launching the App.

## Status

Scaffolded. Use cases not yet implemented.

The App project carries a `.skip-build` marker because the MAUI workload was not available in the environment where this was scaffolded; the Api and tests build.
