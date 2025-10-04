using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTube.Services;
using WinTube.ViewModels;

namespace WinTube.Pages;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = ((App)Application.Current).Container.GetService<MainViewModel>();

    public MainPage()
    {
        InitializeComponent();
        ((App)Application.Current).Container.GetService<NavigationService>().Initialize(contentFrame, playerFrame);

        contentFrame.Navigated += OnContentFrameNavigated;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => contentFrame.SourcePageType = typeof(HomePage);

    private void OnContentFrameNavigated(object sender, NavigationEventArgs e) => ViewModel.IsTitleBarVisible = e.SourcePageType != typeof(ViewPage);

    bool state;

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (state)
            VisualStateManager.GoToState(this, "FullPlayer", true);
        else
            VisualStateManager.GoToState(this, "MiniPlayer", true);

        state = !state;
    }
}