using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WinTube.Controls;

public sealed partial class VideoDurationBadge : UserControl
{
    public VideoDurationBadge()
    {
        this.InitializeComponent();
        UpdateVisuals();
    }

    public TimeSpan? Duration
    {
        get => (TimeSpan?)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(
            nameof(Duration),
            typeof(TimeSpan?),
            typeof(VideoDurationBadge),
            new PropertyMetadata(null, OnDurationChanged));

    private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VideoDurationBadge control)
            control.UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Duration == null)
        {
            RootBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            DurationText.Text = "LIVE";
        }
        else
        {
            RootBorder.Background = new SolidColorBrush(Windows.UI.Colors.Black);
            DurationText.Text = Duration.Value.ToString(@"hh\:mm\:ss");

            if (Duration.Value.Hours > 0)
                DurationText.Text = Duration.Value.ToString(@"hh\:mm\:ss");
            else
                DurationText.Text = Duration.Value.ToString(@"mm\:ss");
        }
    }
}