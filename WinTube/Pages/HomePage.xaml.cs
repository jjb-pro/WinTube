using LibVLCSharp.Shared;
using Windows.UI.Xaml.Controls;

namespace WinTube.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        Core.Initialize();
        InitializeComponent();
    }
}