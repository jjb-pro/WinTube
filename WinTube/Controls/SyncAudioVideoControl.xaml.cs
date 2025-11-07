using AsyncAwaitBestPractices;
using DependencyPropertyGenerator;
using LibVLCSharp.Shared;
using System;
using System.Collections;
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
    private readonly YoutubeClient _youtubeClient;
    private LibVLC _libVLC;
    private MediaPlayer _mediaPlayer;
    private SystemMediaTransportControls _smtc;
    private DispatcherTimer _hideTimer;
    //private Storyboard _fadeInStoryboard;
    //private Storyboard _fadeOutStoryboard;
    private bool _isSeeking;
    private long _currentTimeMs;

    public ObservableCollection<NamedMediaSource> SubtitleSelections { get; } = new ObservableCollection<NamedMediaSource>();

    public SyncAudioVideoControl()
    {
        this.InitializeComponent();
        _youtubeClient = new YoutubeClient();

        Core.Initialize();

        _smtc = SystemMediaTransportControls.GetForCurrentView();
        _smtc.IsEnabled = true;
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsStopEnabled = true;
        _smtc.IsNextEnabled = false;
        _smtc.IsPreviousEnabled = false;
        _smtc.ButtonPressed += Smtc_ButtonPressed;

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _hideTimer.Tick += HideTimer_Tick;

        //_fadeInStoryboard = new Storyboard();
        //var fadeInAnim = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromSeconds(0.3) };
        //Storyboard.SetTarget(fadeInAnim, overlays);
        //Storyboard.SetTargetProperty(fadeInAnim, "Opacity");
        //_fadeInStoryboard.Children.Add(fadeInAnim);

        //_fadeOutStoryboard = new Storyboard();
        //var fadeOutAnim = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(0.3) };
        //Storyboard.SetTarget(fadeOutAnim, overlays);
        //Storyboard.SetTargetProperty(fadeOutAnim, "Opacity");
        //_fadeOutStoryboard.Children.Add(fadeOutAnim);
        //_fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    partial void OnVideoStreamsChanged()
    {
        // select the best streams
        SelectedAudioSource = AudioStreams.LastOrDefault();
        SelectedVideoSource = VideoStreams.LastOrDefault();

        if (null == VideoStreams || !VideoStreams.Any())
            return;

        CreateMediaAsync().SafeFireAndForget(ex => Debug.WriteLine($"Could not create media player: {ex}"));
    }







    private void OnLoaded(object sender, RoutedEventArgs e) => ResetTimer();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _hideTimer?.Stop();
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

    private void OnVideoPlayerElement_Initialized(object sender, LibVLCSharp.Platforms.UWP.InitializedEventArgs e)
    {
        _libVLC = new LibVLC(true, e.SwapChainOptions);
    }

    private async Task CreateMediaAsync()
    {
        if (_libVLC == null || SelectedVideoSource == null)
            return;

        _currentTimeMs = _mediaPlayer?.Time ?? 0;

        DisposePlayer();

        var media = new Media(_libVLC, SelectedVideoSource.Url, FromType.FromLocation);

        if (SelectedAudioSource != null)
        {
            media.AddOption($":input-slave={SelectedAudioSource.Url}");
        }

        //if (SelectedSubtitle?.Source != null)
        //{
        //    media.AddOption($":sub-file={SelectedSubtitle.Source.Url}");
        //}

        _mediaPlayer = new MediaPlayer(media);
        _mediaPlayer.TimeChanged += OnTimeChanged;
        _mediaPlayer.LengthChanged += OnLengthChanged;
        _mediaPlayer.Playing += OnPlaying;
        videoPlayerElement.MediaPlayer = _mediaPlayer;


        _mediaPlayer.EncounteredError += (s, ev) =>
        {
            System.Diagnostics.Debug.WriteLine("LibVLC: EncounteredError");
        };
        _mediaPlayer.Stopped += (s, ev) =>
        {
            System.Diagnostics.Debug.WriteLine("LibVLC: Stopped");
        };
        _mediaPlayer.EndReached += (s, ev) =>
        {
            System.Diagnostics.Debug.WriteLine("LibVLC: EndReached");
        };

        _mediaPlayer.Play();

        //UpdatePlayPauseIcon();
    }

    private void OnPlaying(object sender, EventArgs e)
    {
        _mediaPlayer.Time = _currentTimeMs;
    }

    private async void OnTimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
    {
        //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //{
        //    if (!_isSeeking)
        //    {
        //        seekSlider.Value = e.Time / 1000.0;
        //    }

        //    var t = TimeSpan.FromMilliseconds(e.Time);
        //    timeElapsedText.Text = t.ToString(t.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        //    var r = TimeSpan.FromMilliseconds(_mediaPlayer.Length - e.Time);
        //    timeRemainingText.Text = r.ToString(r.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
        //});
    }

    private async void OnLengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
    {
        //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //{
        //    seekSlider.Maximum = e.Length / 1000.0;
        //});
    }

    private void SeekSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_isSeeking && _mediaPlayer != null)
        {
            _mediaPlayer.Time = (long)(e.NewValue * 1000);
        }
    }

    private void SeekSlider_ThumbDragStarted(object sender, Windows.UI.Xaml.Controls.Primitives.DragStartedEventArgs e)
    {
        _isSeeking = true;
    }

    private void SeekSlider_ThumbDragCompleted(object sender, Windows.UI.Xaml.Controls.Primitives.DragCompletedEventArgs e)
    {
        _isSeeking = false;
    }

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
        if (_mediaPlayer == null) return;

        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Pause();
        }
        else
        {
            _mediaPlayer.Play();
        }
        UpdatePlayPauseIcon();
    }

    private void UpdatePlayPauseIcon()
    {
        //if (_mediaPlayer?.IsPlaying ?? false)
        //{
        //    playPauseIcon.Glyph = "&#xE769;"; // Pause
        //}
        //else
        //{
        //    playPauseIcon.Glyph = "&#xE768;"; // Play
        //}
    }

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
        //_fadeOutStoryboard.Stop();
        //_fadeInStoryboard.Begin();
        //overlays.IsHitTestVisible = true;
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    private void HideTimer_Tick(object sender, object e)
    {
    //    _fadeInStoryboard.Stop();
    //    _fadeOutStoryboard.Begin();
    }

    private void FadeOutStoryboard_Completed(object sender, object e)
    {
        //overlays.IsHitTestVisible = false;
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
    }

    //protected override void OnVisibilityChanged(DependencyObject sender, Windows.UI.Xaml.DependencyPropertyChangedEventArgs args)
    //{
    //    base.OnVisibilityChanged(sender, args);
    //    if ((Visibility)args.NewValue == Visibility.Collapsed)
    //    {
    //        _mediaPlayer?.Stop();
    //    }
    //}
}