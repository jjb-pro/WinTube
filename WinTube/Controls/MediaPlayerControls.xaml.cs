using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using DependencyPropertyGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using WinTube.Model;

#nullable enable

namespace WinTube.Controls;

[ObservableObject]
[DependencyProperty<IEnumerable<INamedStreamSource>>("AudioSources")]
[DependencyProperty<IEnumerable<INamedStreamSource>>("VideoSources")]
[DependencyProperty<IEnumerable<INamedStreamSource>>("SubtitleSources")]
public sealed partial class MediaPlayerControls : UserControl
{
    private DispatcherTimer _hideTimer;
    private Storyboard _fadeInStoryboard;
    private Storyboard _fadeOutStoryboard;
    private long _currentTimeMs;

    [ObservableProperty] public partial bool IsSubtitleOn { get; set; }
    [ObservableProperty] public partial IEnumerable<INamedStreamSource>? Subtitles { get; set; }
    [ObservableProperty] public partial INamedStreamSource? SelectedSubtitle { get; set; }

    [ObservableProperty] public partial INamedStreamSource? SelectedAudioSource { get; set; }
    [ObservableProperty] public partial INamedStreamSource? SelectedVideoSource { get; set; }

    public MediaPlayerControls()
    {
        InitializeComponent();

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5f) };
        _hideTimer.Tick += HideTimer_Tick;

        _fadeInStoryboard = new Storyboard();
        var fadeInAnim = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromSeconds(0.3) };
        Storyboard.SetTarget(fadeInAnim, overlays);
        Storyboard.SetTargetProperty(fadeInAnim, "Opacity");
        _fadeInStoryboard.Children.Add(fadeInAnim);

        _fadeOutStoryboard = new Storyboard();
        var fadeOutAnim = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(0.3) };
        Storyboard.SetTarget(fadeOutAnim, overlays);
        Storyboard.SetTargetProperty(fadeOutAnim, "Opacity");
        _fadeOutStoryboard.Children.Add(fadeOutAnim);
        _fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    async partial void OnVideoSourcesChanged()
    {
        // select the best streams
        SelectedAudioSource = AudioSources.FirstOrDefault();
        SelectedVideoSource = VideoSources.LastOrDefault();

        if (null == SelectedAudioSource || null == SelectedVideoSource)
            return;

        parentPlayer.SetSources(SelectedAudioSource, SelectedVideoSource, SubtitleSources);
    }

    partial void OnSelectedAudioSourceChanged(INamedStreamSource? oldValue, INamedStreamSource? newValue)
    {
        Debug.WriteLine(oldValue);
        Debug.WriteLine(newValue);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        seekBar.SeekRequested += SeekBar_SeekRequested;
        ResetTimer();
    }

    private void SeekBar_SeekRequested(object sender, SeekRequestedEventArgs e)
    {
        Debug.WriteLine($"Seek requested to {e.Position}");
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _hideTimer?.Stop();
    }

    async partial void OnSubtitleSourcesChanged()
    {
        if (null == SubtitleSources || !SubtitleSources.Any())
        {
            Subtitles = null;
        }
        else
        {
            var offSubtitle = new NamedUriStreamSource("Off", null!);
            Subtitles = [offSubtitle, .. SubtitleSources];
            SelectedSubtitle = offSubtitle;
        }
    }

    private void OnPositionChanged(object sender, TimeSpan newPosition)
    {
        // The seek bar benefits from updating every tick (~30fps) for smooth motion, but
        // reformatting two strings that often is wasted work — nobody can read a label
        // changing 30x/sec. Throttle text updates to ~4x/sec instead.
        seekBar.Position = newPosition;

        var nowMs = (long)newPosition.TotalMilliseconds;
        if (nowMs != 0 && Math.Abs(nowMs - _currentTimeMs) < 250)
            return;
        _currentTimeMs = nowMs;

        var r = parentPlayer.Length - newPosition;
        timeElapsedText.Text = newPosition.ToString(newPosition.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        timeRemainingText.Text = r.ToString(r.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
    }

    private void OnSeekRequested(object sender, SeekRequestedEventArgs e) => parentPlayer.SeekTo(e.Position);

    private void OnPlayPauseClicked(object sender, RoutedEventArgs e)
    {
        if (parentPlayer.IsPlaying)
            parentPlayer.Pause();
        else
            parentPlayer.Play();

        UpdatePlayPauseIcon();
    }

    private void UpdatePlayPauseIcon() => playPauseIcon.Glyph = parentPlayer.IsPlaying ? "\uE769" : "\uE768";

    private void OnFullscreenButtonClicked(object sender, RoutedEventArgs e)
    {
        var view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
            view.ExitFullScreenMode();
        else
            view.TryEnterFullScreenMode();
    }

    private void OnMainGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        // YouTube-style double-tap: left half skips back, right half skips forward.
        var x = e.GetPosition(mainGrid).X;
        var delta = x < mainGrid.ActualWidth / 2
            ? TimeSpan.FromSeconds(-10)
            : TimeSpan.FromSeconds(10);

        parentPlayer.SeekBy(delta);
        ResetTimer();
    }

    private void OnSpeedSelected(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string tagValue } &&
            double.TryParse(tagValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var rate))
        {
            parentPlayer.SetPlaybackRate(rate);
        }
    }

    private void OnMainGridPointerMoved(object sender, PointerRoutedEventArgs e) => ResetTimer();

    private void ResetTimer()
    {
        _fadeOutStoryboard.Stop();
        _fadeInStoryboard.Begin();
        overlays.IsHitTestVisible = true;
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    private void HideTimer_Tick(object sender, object e)
    {
        _fadeInStoryboard.Stop();
        _fadeOutStoryboard.Begin();
    }

    private void FadeOutStoryboard_Completed(object sender, object e)
    {
        overlays.IsHitTestVisible = false;
    }

    private void OnSubtitleButtonClick(object sender, RoutedEventArgs e) => IsSubtitleOn = !IsSubtitleOn;

    private void OnSelectedAudioSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (null == SelectedAudioSource)
            return;

        parentPlayer.SetAudioSource(SelectedAudioSource, true);
    }

    private void OnSelectedVideoSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (null == SelectedVideoSource)
            return;

        parentPlayer.SetVideoSource(SelectedVideoSource, SubtitleSources);
    }
}