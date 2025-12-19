using AsyncAwaitBestPractices;
using DependencyPropertyGenerator;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using WinTube.Model;
using YoutubeExplode;

namespace WinTube.Controls;

[DependencyProperty<IEnumerable<NamedMediaSource>>("AudioStreams")]
[DependencyProperty<IEnumerable<NamedMediaSource>>("VideoStreams")]
[DependencyProperty<IEnumerable<NamedMediaSource>>("CaptionStreams")]
[DependencyProperty<NamedMediaSource>("SelectedAudioSource")]
[DependencyProperty<NamedMediaSource>("SelectedVideoSource")]
//[DependencyProperty<SelectableMediaSource>("SelectedSubtitle")]
public sealed partial class SyncAudioVideoControl : UserControl
{
    private LibVLC _libVLC;
    private MediaPlayer _mediaPlayer;
    private SystemMediaTransportControls _smtc;
    private DispatcherTimer _hideTimer;
    private Storyboard _fadeInStoryboard;
    private Storyboard _fadeOutStoryboard;
    private long _currentTimeMs;

    public ObservableCollection<NamedMediaSource> SubtitleSelections { get; } = [];

    public SyncAudioVideoControl()
    {
        InitializeComponent();
        Core.Initialize();

        _smtc = SystemMediaTransportControls.GetForCurrentView();
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsStopEnabled = true;
        _smtc.IsNextEnabled = false;
        _smtc.IsPreviousEnabled = false;
        _smtc.ButtonPressed += Smtc_ButtonPressed;

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

    partial void OnVideoStreamsChanged()
    {
        // select the best streams
        SelectedAudioSource = AudioStreams.LastOrDefault();
        SelectedVideoSource = VideoStreams.FirstOrDefault();

        if (null == VideoStreams || !VideoStreams.Any())
            return;

        CreateMediaAsync().SafeFireAndForget(ex => Debug.WriteLine($"Could not create media player: {ex}"));
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
        _mediaPlayer?.Stop();
        _smtc.ButtonPressed -= Smtc_ButtonPressed;
        DisposePlayer();
    }

    private void OnCaptionStreamsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //SubtitleSelections.Clear();
        //SubtitleSelections.Add(new NamedMediaSource("Off", null));
        //foreach (var caption in CaptionStreams)
        //{
        //    SubtitleSelections.Add(new NamedMediaSource(caption.ToString(), caption));
        //}

        //if (SelectedSubtitle == null && SubtitleSelections.Count > 0)
        //{
        //    SelectedSubtitle = SubtitleSelections[0]; // Off by default
        //}
    }

    private void OnVideoViewInitialized(object sender, InitializedEventArgs e)
    {
        _libVLC = new LibVLC(false, e.SwapChainOptions);
        _mediaPlayer = new MediaPlayer(_libVLC);
        _mediaPlayer.TimeChanged += OnTimeChanged;
        _mediaPlayer.LengthChanged += OnLengthChanged;

        videoView.MediaPlayer = _mediaPlayer;
    }

    private async Task CreateMediaAsync()
    {
        using var media = new Media(_libVLC, SelectedVideoSource.Url, FromType.FromLocation);
        media.AddOption($":input-slave={SelectedAudioSource.Url}");

        _mediaPlayer.Play(media);

        UpdatePlayPauseIcon();
    }

    private void OnPlaying(object sender, EventArgs e)
    {
        _mediaPlayer.Time = _currentTimeMs;
    }

    private async void OnTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            seekBar.Position = TimeSpan.FromMilliseconds(e.Time);

            var t = TimeSpan.FromMilliseconds(e.Time);
            timeElapsedText.Text = t.ToString(t.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
            var r = TimeSpan.FromMilliseconds(_mediaPlayer.Length - e.Time);
            timeRemainingText.Text = r.ToString(r.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        });
    }

    private async void OnLengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            seekBar.Duration = TimeSpan.FromMilliseconds(e.Length);
        });
    }

    private void OnSeekRequested(object sender, SeekRequestedEventArgs e) => _mediaPlayer?.Time = (long)e.Position.TotalMilliseconds;

    private async void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    _mediaPlayer?.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    _mediaPlayer?.Pause();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    _mediaPlayer?.Stop();
                    break;
            }
            UpdatePlayPauseIcon();
        });
    }

    private void OnPlayPauseClicked(object sender, RoutedEventArgs e)
    {
        if (_mediaPlayer == null)
            return;

        if (_mediaPlayer.IsPlaying)
            _mediaPlayer.Pause();
        else
            _mediaPlayer.Play();

        UpdatePlayPauseIcon();
    }

    private void UpdatePlayPauseIcon() => playPauseIcon.Glyph = _mediaPlayer?.IsPlaying ?? false ? "\uE769" : "\uE768";

    private void OnFullscreenButtonClicked(object sender, RoutedEventArgs e)
    {
        _mediaPlayer?.ToggleFullscreen();
    }

    private void MainGrid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ResetTimer();
    }

    private void MainGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        ResetTimer();
    }

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

    private void DisposePlayer()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.TimeChanged -= OnTimeChanged;
            _mediaPlayer.LengthChanged -= OnLengthChanged;
            _mediaPlayer.Playing -= OnPlaying;
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
            _mediaPlayer = null;
        }

        _libVLC?.Dispose();
    }
}