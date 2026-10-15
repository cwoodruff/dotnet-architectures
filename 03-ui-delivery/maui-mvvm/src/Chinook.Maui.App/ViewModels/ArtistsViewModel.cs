using System.Collections.ObjectModel;
using Chinook.Maui.App.Models;
using Chinook.Maui.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chinook.Maui.App.ViewModels;

/// <summary>
/// The page's state and behavior, with no reference to any view type. The CommunityToolkit.Mvvm source
/// generators turn the [ObservableProperty] fields into change-notifying properties and the [RelayCommand]
/// method into LoadArtistsCommand, which the page binds to.
/// </summary>
public partial class ArtistsViewModel(ArtistsApiClient api) : ObservableObject
{
    public ObservableCollection<ArtistItem> Artists { get; } = [];

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    [RelayCommand]
    private async Task LoadArtistsAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var artists = await api.GetTopArtistsAsync(cancellationToken);
            Artists.Clear();
            foreach (var artist in artists)
            {
                Artists.Add(artist);
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Could not reach the Api: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
