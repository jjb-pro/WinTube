using System;
using System.Collections.ObjectModel;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinTube.Model;

namespace WinTube.Controls
{
    public sealed partial class SynchronizedMediaControl : UserControl
    {
        private readonly SystemMediaTransportControls _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();

        private bool _isPlaying = false;

        private readonly MediaPlayer _audioPlayer;
        private readonly MediaPlayer _videoPlayer;

        private readonly MediaTimelineController _mediaTimelineController = new();

        // dependency properties for audio, video, and caption sources
        public ObservableCollection<NamedMediaSource> AudioStreams
        {
            get => (ObservableCollection<NamedMediaSource>)GetValue(AudioStreamsProperty);
            set => SetValue(AudioStreamsProperty, value);
        }

        public static readonly DependencyProperty AudioStreamsProperty =
            DependencyProperty.Register(
                nameof(AudioStreams),
                typeof(ObservableCollection<NamedMediaSource>),
                typeof(SynchronizedMediaControl),
                new PropertyMetadata(new ObservableCollection<NamedMediaSource>()));

        public ObservableCollection<NamedMediaSource> VideoStreams
        {
            get => (ObservableCollection<NamedMediaSource>)GetValue(VideoStreamsProperty);
            set => SetValue(VideoStreamsProperty, value);
        }

        public static readonly DependencyProperty VideoStreamsProperty =
            DependencyProperty.Register(
                nameof(VideoStreams),
                typeof(ObservableCollection<NamedMediaSource>),
                typeof(SynchronizedMediaControl),
                new PropertyMetadata(null));

        public ObservableCollection<NamedCaptionSource> CaptionSources
        {
            get => (ObservableCollection<NamedCaptionSource>)GetValue(CaptionSourcesProperty);
            set => SetValue(CaptionSourcesProperty, value);
        }

        public static readonly DependencyProperty CaptionSourcesProperty =
            DependencyProperty.Register(
                nameof(CaptionSources),
                typeof(ObservableCollection<NamedCaptionSource>),
                typeof(SynchronizedMediaControl),
                new PropertyMetadata(null));

        public ImageSource PosterSource
        {
            get => (ImageSource)GetValue(PosterSourceProperty);
            set => SetValue(PosterSourceProperty, value);
        }

        public static readonly DependencyProperty PosterSourceProperty =
            DependencyProperty.Register(
                nameof(PosterSource),
                typeof(ImageSource),
                typeof(SynchronizedMediaControl),
                new PropertyMetadata(null, OnPosterSourceChanged));

        public SynchronizedMediaControl()
        {
            InitializeComponent();

            _systemMediaTransportControls.IsEnabled = true;
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.ButtonPressed += OnSystemMediaTransportControlsButtonPressed;

            _audioPlayer = new MediaPlayer
            {
                IsMuted = false,
                TimelineController = _mediaTimelineController,
                CommandManager =
                {
                    IsEnabled = false
                }
            };
            _videoPlayer = new MediaPlayer
            {
                TimelineController = _mediaTimelineController,
                CommandManager =
                {
                    IsEnabled = false
                }
            };

            _mediaTimelineController.PositionChanged += OnPositionChanged;
            _videoPlayer.CurrentStateChanged += OnVideoPlayerStateChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            audioPlayerElement.SetMediaPlayer(_audioPlayer);
            videoPlayerElement.SetMediaPlayer(_videoPlayer);

            _videoPlayer.MediaOpened += OnMediaOpened;

            mediaTransportControls.AudioStreamChanged += OnAudioStreamChanged;
            mediaTransportControls.VideoStreamChanged += OnVideoStreamChanged;
            mediaTransportControls.CaptionChanged += OnCaptionChanged;
        }

        private static void OnPosterSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SynchronizedMediaControl control)
                control.videoPlayerElement.PosterSource = e.NewValue as ImageSource;
        }

        private async void OnPositionChanged(MediaTimelineController sender, object args)
        {
            var percentage = sender.Position.TotalSeconds / _videoPlayer.PlaybackSession.NaturalDuration.TotalSeconds * 100;
            if (!double.IsNaN(percentage) && percentage >= 0 && percentage <= 100)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => mediaTransportControls.SetProgressSliderPosition(percentage));
        }

        private async void OnVideoPlayerStateChanged(MediaPlayer sender, object args)
        {
            var isBufferingCompleted = sender.PlaybackSession.PlaybackState != MediaPlaybackState.Opening && sender.PlaybackSession.PlaybackState != MediaPlaybackState.Buffering;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mediaTransportControls.SetBufferingState(isBufferingCompleted);
            });
        }

        // media opening
        private async void OnAudioStreamChanged(object sender, NamedMediaSource e) => _audioPlayer.Source = MediaSource.CreateFromStream(await e.GetStreamCallback(), "audio/mp3");

        private async void OnVideoStreamChanged(object sender, NamedMediaSource e)
        {
            var videoStream = await e.GetStreamCallback();
            var mediaSource = MediaSource.CreateFromStream(videoStream, "video/mp4");

            foreach (var captionSource in CaptionSources)
            {
                var timedTextSource = TimedTextSource.CreateFromUri(captionSource.Uri);
                mediaSource.ExternalTimedTextSources.Add(timedTextSource);
            }

            _videoPlayer.Source = new MediaPlaybackItem(mediaSource);
        }

        private void OnCaptionChanged(object sender, int e)
        {
            if (_videoPlayer.Source is not MediaPlaybackItem playbackItem)
                return;

            for (uint i = 0; i < playbackItem.TimedMetadataTracks.Count; i++)
                playbackItem.TimedMetadataTracks.SetPresentationMode(i, i == e ? TimedMetadataTrackPresentationMode.PlatformPresented : TimedMetadataTrackPresentationMode.Disabled);
        }

        private void OnMediaOpened(MediaPlayer sender, object args)
        {
            // ToDo: set real information
            var updater = _systemMediaTransportControls.DisplayUpdater;

            updater.Type = MediaPlaybackType.Video;
            updater.VideoProperties.Title = "artist";
            updater.VideoProperties.Subtitle = "album artist";

            // Update the system media transport controls
            updater.Update();
        }

        // play/pause logic
        private void OnPlayPauseButtonClicked(object sender, RoutedEventArgs e) => TogglePlayState();

        private async void OnSystemMediaTransportControlsButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (args.Button == SystemMediaTransportControlsButton.Play || args.Button == SystemMediaTransportControlsButton.Pause)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, TogglePlayState);
        }

        private void TogglePlayState()
        {
            if (_isPlaying)
                _mediaTimelineController.Pause();
            else
                _mediaTimelineController.Resume();

            _isPlaying = !_isPlaying;
        }

        // rewind logic
        private void OnProgressSliderValueChanged(object sender, double value) => _mediaTimelineController.Position = TimeSpan.FromSeconds(value / 100 * _videoPlayer.PlaybackSession.NaturalDuration.TotalSeconds);

        private void MediaPlayerDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (sender is not FrameworkElement mediaElement)
                return;

            mediaTransportControls.SetBufferingState(false);

            if (e.GetPosition(mediaElement).X > mediaElement.ActualWidth / 2)
                _mediaTimelineController.Position += TimeSpan.FromSeconds(30);
            else
                _mediaTimelineController.Position += TimeSpan.FromSeconds(-10);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _mediaTimelineController.Pause();

            _audioPlayer?.Dispose();
            _videoPlayer?.Dispose();
        }
    }
}