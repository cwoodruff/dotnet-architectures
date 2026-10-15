using Chinook.Maui.App.ViewModels;

namespace Chinook.Maui.App.Views;

public partial class ArtistsPage : ContentPage
{
    public ArtistsPage(ArtistsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
