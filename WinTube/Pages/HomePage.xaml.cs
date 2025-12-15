using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System.Linq;
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