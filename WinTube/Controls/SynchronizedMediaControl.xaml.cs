using System;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace WinTube.Controls
{
    public sealed partial class SynchronizedMediaControl : UserControl
    {
        private readonly DispatcherTimer _syncTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };

        //private readonly MediaElement _audioPlayer = new MediaElement();
        //private readonly MediaPlayer _videoPlayer = new MediaPlayer();

        private readonly MediaTimelineController _controller = new MediaTimelineController();

        private readonly SystemMediaTransportControls _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();

        public SynchronizedMediaControl()
        {
            InitializeComponent();

            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnSyncTimerTick(object sender, object e)
        {
            if (videoPlayerElement.CurrentState == MediaElementState.Paused)
            {
                audioPlayerElement.IsMuted = true;
                System.Diagnostics.Debug.WriteLine("Video is paused. Audio is muted.");
                return;
            }
            else
            {
                audioPlayerElement.IsMuted = false;
            }

            if (videoPlayerElement.CurrentState == MediaElementState.Buffering)
            {
                audioPlayerElement.IsMuted = true;
                System.Diagnostics.Debug.WriteLine("Video is Buffering. Audio is muted.");
                return;
            }
            else
            {
                audioPlayerElement.IsMuted = false;
            }

            double positionDifference = Math.Abs(videoPlayerElement.Position.TotalSeconds - audioPlayerElement.Position.TotalSeconds);

            if (positionDifference > 0.1)
            {
                audioPlayerElement.Position = videoPlayerElement.Position;
                System.Diagnostics.Debug.WriteLine($"Sync correction applied. Audio Player position adjusted to {audioPlayerElement.Position} to match Video Player.");
            }

            if (audioPlayerElement.CurrentState != MediaElementState.Playing)
            {
                audioPlayerElement.Play();
                audioPlayerElement.IsMuted = false;
                System.Diagnostics.Debug.WriteLine("Audio Player started playing.");
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //audioPlayerElement.Dispose();
            //_videoPlayer.Dispose();
        }

        public void SetSources(MediaSource audioSource, MediaSource videoSource)
        {
            audioPlayerElement.SetPlaybackSource(audioSource);
            videoPlayerElement.SetPlaybackSource(videoSource);

            _syncTimer.Start();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            videoPlayerElement.MediaOpened += OnMediaOpened;
            //videoPlayerElement.CurrentStateChanged

            //videoPlayerElement.CommandManager.IsEnabled = false;
            //_videoPlayer.TimelineController = _controller;

            //_videoPlayer.CommandManager.IsEnabled = false;
            //_videoPlayer.TimelineController = _controller;

            //audioPlayerElement = (audioPlayerElement);
            //videoPlayerElement.SetMediaStreamSource(_videoPlayer);

            //_videoPlayer.PlaybackSession.PlaybackStateChanged += PlaybackStateChanged;
            //_videoPlayer.PlaybackSession.PlaybackStateChanged += PlaybackStateChanged;

            // synchronization logic
            //_videoPlayer.VolumeChanged += (s, a) => audioPlayerElement.Volume = s.Volume;
            //_videoPlayer.IsMutedChanged += (s, a) => audioPlayerElement.IsMuted = s.IsMuted;

            _syncTimer.Tick += OnSyncTimerTick;
        }

        private void CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            else
                _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
        }

        private void OnMediaOpened(object sender, RoutedEventArgs e)
        {
            // Get the updater.
            SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;

            updater.Type = MediaPlaybackType.Video;
            updater.VideoProperties.Title = "artist";
            updater.VideoProperties.Subtitle = "album artist";

            // Update the system media transport controls
            updater.Update();
        }

        private async void SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _controller.Resume());
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _controller.Pause());
                    break;
                default:
                    break;
            }
        }

        private async void PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //if (audioPlayerElement.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering || _videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering)
                //    mediaTransportControls.SetBufferingState(false);
                //else
                //    mediaTransportControls.SetBufferingState(true);
            }).AsTask();
        }

        private void PlayPauseButtonClicked(object sender, RoutedEventArgs e)
        {
            //if (audioPlayerElement.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering || _videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering)
            //    return;

            if (_syncTimer.IsEnabled)
            {
                _syncTimer.Stop();
                audioPlayerElement.Pause();
                videoPlayerElement.Pause();
            }
            else
            {
                _syncTimer.Start();
                audioPlayerElement.Play();
                videoPlayerElement.Play();
            }
        }

        private void ProgressSliderValueChanged(object sender, double value)
        {
            if (!videoPlayerElement.NaturalDuration.HasTimeSpan)
                return;

            var totalSeconds = videoPlayerElement.NaturalDuration.TimeSpan.TotalSeconds;
            videoPlayerElement.Position = TimeSpan.FromSeconds(value * totalSeconds);
        }

        private void MediaPlayerDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!(sender is FrameworkElement mediaElement))
                return;

            var tapPosition = e.GetPosition(mediaElement);

            if (tapPosition.X > mediaElement.ActualWidth / 2)
                videoPlayerElement.Position += TimeSpan.FromSeconds(10);
            else
                videoPlayerElement.Position += TimeSpan.FromSeconds(-30);
        }
    }
}