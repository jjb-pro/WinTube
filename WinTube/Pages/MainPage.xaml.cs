using WinTube.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTube.Pages;
using WinTube.Services;

namespace WinTube.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = ((App)Application.Current).Container.GetService<MainViewModel>();

        public MainPage()
        {
            InitializeComponent();
            ((App)Application.Current).Container.GetService<NavigationService>().Initialize(contentFrame);
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => contentFrame.SourcePageType = typeof(HomePage);
    }
}