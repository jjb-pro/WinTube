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

    private IEnumerable<IRandomAccessStream> _subtitleStreams = [];
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

        parentPlayer.SetSourcesAsync(await SelectedAudioSource.GetStreamAsync(), await SelectedVideoSource.GetStreamAsync(), _subtitleStreams)
            .SafeFireAndForget(ex => Debug.WriteLine("Could not set media sources: " + ex.Message));
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
        foreach (var stream in _subtitleStreams)
            stream.Dispose();

        if (null == SubtitleSources)
        {
            Subtitles = null;
        }
        else
        {
            _subtitleStreams = await Task.WhenAll(
                SubtitleSources.Select(async s => await s.GetStreamAsync())
            );

            var offSubtitle = new NamedUriStreamSource("Off", null!);
            Subtitles = [offSubtitle, .. SubtitleSources];
            SelectedSubtitle = offSubtitle;
        }
    }

    private void OnPositionChanged(object sender, TimeSpan newPosition)
    {
        seekBar.Position = newPosition;

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

    private void OnFullscreenButtonClicked(object sender, RoutedEventArgs e) { }

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

    private async void OnSelectedAudioSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (null == SelectedAudioSource)
            return;

        parentPlayer.SetAudioSourceAsync(await SelectedAudioSource.GetStreamAsync(), true)
            .SafeFireAndForget(ex => Debug.WriteLine("Could not set audio source: " + ex.Message));
    }

    private async void OnSelectedVideoSourceChanged(object sender, SelectionChangedEventArgs e)
    {
        if (null == SelectedVideoSource)
            return;

        parentPlayer.SetVideoSourceAsync(await SelectedVideoSource.GetStreamAsync(), _subtitleStreams)
            .SafeFireAndForget(ex => Debug.WriteLine("Could not set video source: " + ex.Message));
    }
}