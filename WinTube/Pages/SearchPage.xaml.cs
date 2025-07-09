using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTube.Model.Observable;
using WinTube.ViewModels;

namespace WinTube.Pages;

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; } = ((App)Application.Current).Container.GetRequiredService<SearchViewModel>();

    public SearchPage() => InitializeComponent();
}