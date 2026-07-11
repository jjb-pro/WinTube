using DependencyPropertyGenerator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTube.Model;

#nullable enable

namespace WinTube.Controls;

[DependencyProperty<bool>("IsBuffering")]
[DependencyProperty<TimeSpan>("Length")]
[DependencyProperty<TimeSpan>("Position")]
public sealed partial class SynchronizedMediaPlayer : MediaPlayerElement
{
    private readonly MediaPlayer _audioPlayer = new()
    {
        AutoPlay = true
    };
    private readonly MediaPlayer _videoPlayer = new()
    {
        Volume = 0, // mute video audio
        CommandManager = { IsEnabled = false } // disable automatic SMTC integration
    };

    private ThreadPoolTimer? _syncTimer;

    // keeps the screen from locking mid-playback
    private readonly DisplayRequest _displayRequest = new();
    private bool _displayRequestActive;

    private MediaSource? _audioSource;
    private MediaSource? _videoSource;

    private const double DeadbandMs = 20;
    private const double HardResyncThresholdMs = 220;

    private const double MaxRateAdjust = 0.35;              // max correction added on top of user rate
    private const double ProportionalGain = 1.0 / 400.0;    // ms of drift -> rate correction
    private const double RateSlewPerTick = 0.05;            // caps how fast the applied rate can change

    private volatile bool _isAudioBuffering;
    private volatile bool _isVideoBuffering;

    private double _appliedCorrection;
    private double _playbackRate = 1.0;

    public SynchronizedMediaPlayer()
    {
        _audioPlayer.CurrentStateChanged += OnAudioPlayerStateChanged;

        _audioPlayer.PlaybackSession.BufferingStarted += OnAudioPlayerBufferingStarted;
        _audioPlayer.PlaybackSession.BufferingEnded += OnAudioPlayerBufferingEnded;

        _videoPlayer.PlaybackSession.BufferingStarted += OnVideoPlayerBufferingStarted;
        _videoPlayer.PlaybackSession.BufferingEnded += OnVideoPlayerBufferingEnded;

        // ToDo: move to separate class
        var smtc = _audioPlayer.SystemMediaTransportControls;
        smtc.IsPlayEnabled = true;
        smtc.IsPauseEnabled = true;
        smtc.ButtonPressed += OnSmtcButtonPressed;

        Unloaded += OnUnloaded;
        SetMediaPlayer(_videoPlayer);
    }

    private async void OnSmtcButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        switch (args.Button)
        {
            case SystemMediaTransportControlsButton.Play:
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Play);
                break;
            case SystemMediaTransportControlsButton.Pause:
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Pause);
                break;
        }
    }

    // synchronize video to audio playback state
    private async void OnAudioPlayerStateChanged(MediaPlayer sender, object args)
    {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            switch (_audioPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    _videoPlayer.Play();
                    StartSyncTimer();
                    RequestKeepScreenOn(true);
                    _audioPlayer.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaPlaybackState.Paused:
                    _videoPlayer.Pause();
                    StopSyncTimer();
                    RequestKeepScreenOn(false);
                    _audioPlayer.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
            }
        });
    }

    private void RequestKeepScreenOn(bool keepOn)
    {
        // DisplayRequest is reference-counted, so guard against double-request/double-release
        if (keepOn && !_displayRequestActive)
        {
            _displayRequest.RequestActive();
            _displayRequestActive = true;
        }
        else if (!keepOn && _displayRequestActive)
        {
            _displayRequest.RequestRelease();
            _displayRequestActive = false;
        }
    }

    // buffering handlers
    private async void OnAudioPlayerBufferingStarted(MediaPlaybackSession sender, object args)
    {
        _isAudioBuffering = true;
        await UpdateBufferingStateAsync();
        await StopPlaybackAsync();
    }

    private async void OnAudioPlayerBufferingEnded(MediaPlaybackSession sender, object args)
    {
        _isAudioBuffering = false;
        await UpdateBufferingStateAsync();
        await TryResumePlaybackAsync();
    }

    private async void OnVideoPlayerBufferingStarted(MediaPlaybackSession sender, object args)
    {
        _isVideoBuffering = true;
        await UpdateBufferingStateAsync();
        await StopPlaybackAsync();
    }

    private async void OnVideoPlayerBufferingEnded(MediaPlaybackSession sender, object args)
    {
        _isVideoBuffering = false;
        await UpdateBufferingStateAsync();
        await TryResumePlaybackAsync();
    }

    private async Task UpdateBufferingStateAsync()
        => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsBuffering = _isAudioBuffering || _isVideoBuffering);

    private async Task StopPlaybackAsync() => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    {
        StopSyncTimer();
        _audioPlayer.Pause();
    });

    private async Task TryResumePlaybackAsync()
    {
        if (_isAudioBuffering || _isVideoBuffering || _audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            return;

        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            _videoPlayer.PlaybackSession.Position = _audioPlayer.PlaybackSession.Position;
            _appliedCorrection = 0;
            _audioPlayer.Play();
            StartSyncTimer();
        });
    }

    private void StartSyncTimer()
    {
        if (_syncTimer != null) return;
        _syncTimer = ThreadPoolTimer.CreatePeriodicTimer(OnTimerTick, TimeSpan.FromMilliseconds(33));
    }

    private void StopSyncTimer()
    {
        _syncTimer?.Cancel();
        _syncTimer = null;
    }

    // synchronization logic
    private async void OnTimerTick(ThreadPoolTimer timer)
    {
        double audioPos, videoPos;

        try
        {
            if (_audioPlayer.CurrentState == MediaPlayerState.Closed)
                return;

            audioPos = _audioPlayer.PlaybackSession.Position.TotalMilliseconds;
            videoPos = _videoPlayer.PlaybackSession.Position.TotalMilliseconds;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        var diff = audioPos - videoPos; // positive => video is behind audio

        try
        {
            if (Math.Abs(diff) > HardResyncThresholdMs)
            {
                _videoPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(audioPos);
                _appliedCorrection = 0;
                _videoPlayer.PlaybackSession.PlaybackRate = _playbackRate;
            }
            else
            {
                var targetCorrection = Math.Abs(diff) < DeadbandMs
                    ? 0.0
                    : Math.Clamp(diff * ProportionalGain, -MaxRateAdjust, MaxRateAdjust);

                _appliedCorrection += (targetCorrection - _appliedCorrection) * RateSlewPerTick;
                _videoPlayer.PlaybackSession.PlaybackRate = Math.Clamp(_playbackRate + _appliedCorrection, 0.1, 4.0);
            }
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        var newPosition = TimeSpan.FromMilliseconds(audioPos);

        await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
        {
            Position = newPosition;
            PositionChanged?.Invoke(this, newPosition);
        });
    }

    // methods for controlling playback
    public bool IsPlaying => _audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public event EventHandler<TimeSpan>? PositionChanged;

    public void Play() => _audioPlayer.Play();

    public void Pause() => _audioPlayer.Pause();

    public void Stop()
    {
        _audioPlayer.Pause();
        _audioPlayer.PlaybackSession.Position = TimeSpan.Zero;
        _videoPlayer.PlaybackSession.Position = TimeSpan.Zero;
    }

    public void SeekTo(TimeSpan position)
    {
        _audioPlayer.PlaybackSession.Position = position;
        _videoPlayer.PlaybackSession.Position = position;
        _appliedCorrection = 0;
    }

    public void SeekBy(TimeSpan delta)
    {
        var target = _audioPlayer.PlaybackSession.Position + delta;
        if (target < TimeSpan.Zero)
            target = TimeSpan.Zero;

        if (Length != default && target > Length)
            target = Length;

        SeekTo(target);
    }

    public void SetPlaybackRate(double rate)
    {
        _playbackRate = rate <= 0 ? 1.0 : rate;
        _audioPlayer.PlaybackSession.PlaybackRate = _playbackRate;
        // video rate is re-applied by the next sync tick
    }

    // react to source changes
    public void SetSources(INamedStreamSource audioSource, INamedStreamSource videoSource, IEnumerable<INamedStreamSource>? subtitleSources)
    {
        SetAudioSource(audioSource);
        SetVideoSource(videoSource, subtitleSources);
    }

    public void SetAudioSource(INamedStreamSource audioSource, bool preservePosition = true)
    {
        var previousPosition = Position;

        _audioPlayer.Source = null;
        _audioSource?.Dispose();

        _audioSource = MediaSource.CreateFromUri(audioSource.Uri);
        _audioPlayer.Source = _audioSource;

        if (preservePosition)
            _audioPlayer.PlaybackSession.Position = previousPosition;

        _audioPlayer.PlaybackSession.PlaybackRate = _playbackRate;

        Length = _audioPlayer.PlaybackSession.NaturalDuration;

        // ToDo: move to separate class
        var smtc = _audioPlayer.SystemMediaTransportControls;
        smtc.DisplayUpdater.Type = MediaPlaybackType.Video;
        smtc.DisplayUpdater.Update();
    }

    public void SetVideoSource(INamedStreamSource videoSource, IEnumerable<INamedStreamSource>? subtitleSources)
    {
        _videoPlayer.Source = null;
        _videoSource?.Dispose();

        _videoSource = MediaSource.CreateFromUri(videoSource.Uri);
        if (subtitleSources != null)
        {
            foreach (var subtitleSource in subtitleSources)
            {
                _videoSource.ExternalTimedTextSources.Add(TimedTextSource.CreateFromUri(subtitleSource.Uri, "text/srt"));
            }
        }

        _videoPlayer.Source = _videoSource;
    }

    public void SetSubtitle(uint idx, bool isOn)
    {
        for (int i )
            _videoPlayer.set
    }

    // dispose resources on unload
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopSyncTimer();
        RequestKeepScreenOn(false);

        _audioPlayer.SystemMediaTransportControls.ButtonPressed -= OnSmtcButtonPressed;

        _audioPlayer.Dispose();
        _videoPlayer.Dispose();

        _audioSource?.Dispose();
        _videoSource?.Dispose();
    }
}