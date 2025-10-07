using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace WinTube.Controls;

public sealed partial class Shimmer : UserControl
{
    public Shimmer()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var storyboard = (Storyboard)ShimmerBorder.Resources["ShimmerStoryboard"];
        storyboard.Begin();
    }
}