using DependencyPropertyGenerator;
using FFmpegInteropX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace WinTube.Controls;

[DependencyProperty<bool>("IsBuffering")]
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

    private readonly DispatcherTimer _syncTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(33)
    };

    private readonly MediaSourceConfig _config = new()
    {
        SkipErrors = uint.MaxValue,
        FastSeek = true,
        StreamBufferSize = 10 * 1024 * 1024, // 10 MB
        FFmpegOptions =
            {
                { "reconnect", 1 },
                { "reconnect_streamed", 1 },
                { "reconnect_on_network_error", 1 }
            }
    };
    private FFmpegMediaSource? _audioFFmpegSource;
    private FFmpegMediaSource? _videoFFmpegSource;

    private const double SyncThreshold = 50; // ms
    private const double HardResyncThreshold = 300; // ms

    private const double MaxPlaybackRate = 2.0;
    private const double MinPlaybackRate = 0.5;
    private const double RateAdjustFactor = 0.2;

    private bool _isAudioBuffering = false;
    private bool _isVideoBuffering = false;

    private double _smoothedDiff = 0;
    private double _currentRate = 1.0;

    private const double DiffSmoothingFactor = 0.1;
    private const double RateSmoothingFactor = 0.1;

    public SynchronizedMediaPlayer()
    {
        _syncTimer.Tick += OnTimerTick;

        _audioPlayer.CurrentStateChanged += OnAudioPlayerStateChanged;
        
        // handle audio/video buffering
        _audioPlayer.PlaybackSession.BufferingStarted += OnAudioPlayerBufferingStarted;
        _audioPlayer.PlaybackSession.BufferingEnded += OnAudioPlayerBufferingEnded;

        _videoPlayer.PlaybackSession.BufferingStarted += OnVideoPlayerBufferingStarted;
        _videoPlayer.PlaybackSession.BufferingEnded += OnVideoPlayerBufferingEnded;

        Unloaded += OnUnloaded;
        SetMediaPlayer(_videoPlayer);
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
                    _syncTimer.Start();
                    break;
                case MediaPlaybackState.Paused:
                    _videoPlayer.Pause();
                    _syncTimer.Stop();
                    break;
            }
        });
    }

    // buffering handlers
    private async void OnAudioPlayerBufferingStarted(MediaPlaybackSession sender, object args)
    {
        _isAudioBuffering = true;
        UpdateBufferingState();
        await StopPlaybackAsync();
    }

    private async void OnAudioPlayerBufferingEnded(MediaPlaybackSession sender, object args)
    {
        _isAudioBuffering = false;
        UpdateBufferingState();
        await TryResumePlaybackAsync();
    }

    private async void OnVideoPlayerBufferingStarted(MediaPlaybackSession sender, object args)
    {
        _isVideoBuffering = true;
        UpdateBufferingState();
        await StopPlaybackAsync();
    }

    private async void OnVideoPlayerBufferingEnded(MediaPlaybackSession sender, object args)
    {
        _isVideoBuffering = false;
        UpdateBufferingState();
        await TryResumePlaybackAsync();
    }

    private void UpdateBufferingState() => IsBuffering = _isAudioBuffering || _isVideoBuffering;

    private async Task StopPlaybackAsync() => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    {
        _syncTimer.Stop();
        _audioPlayer.Pause();
    });

    private async Task TryResumePlaybackAsync()
    {
        if (_isAudioBuffering || _isVideoBuffering || _audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            return;

        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
            _audioPlayer.Play();
            _syncTimer.Start();
        });
    }

    // synchronization logic
    private void OnTimerTick(object sender, object e)
    {
        var audioPos = _audioPlayer.PlaybackSession.Position.TotalMilliseconds;
        var videoPos = _videoPlayer.PlaybackSession.Position.TotalMilliseconds;
        var diff = audioPos - videoPos;

        if (Math.Abs(diff) > HardResyncThreshold)
        {
            _videoPlayer.PlaybackSession.Position = TimeSpan.FromMilliseconds(audioPos);

            _currentRate = 1.0;
            _smoothedDiff = 0;
            return;
        }

        // smooth timing error
        _smoothedDiff += (diff - _smoothedDiff) * DiffSmoothingFactor;

        if (Math.Abs(_smoothedDiff) <= SyncThreshold)
        {
            _currentRate += (1.0 - _currentRate) * RateSmoothingFactor;
            _videoPlayer.PlaybackSession.PlaybackRate = _currentRate;
            return;
        }

        var targetRate = Math.Clamp(1.0 + (_smoothedDiff / 1000.0) * RateAdjustFactor, MinPlaybackRate, MaxPlaybackRate);
        _currentRate += (targetRate - _currentRate) * RateSmoothingFactor;

        _videoPlayer.PlaybackSession.PlaybackRate = _currentRate;

        PositionChanged?.Invoke(this, Position);
    }

    // methods for controlling playback
    public bool IsPlaying => _audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

    public TimeSpan Length => _audioPlayer.PlaybackSession.NaturalDuration;
    public event EventHandler<TimeSpan>? LengthChanged;

    public TimeSpan Position => _audioPlayer.PlaybackSession.Position;
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
    }

    // react to source changes
    public Task SetSourcesAsync(IRandomAccessStream audioStream, IRandomAccessStream videoStream, IEnumerable<IRandomAccessStream> subtitleStreams)
        => Task.WhenAll(SetAudioSourceAsync(audioStream), SetVideoSourceAsync(videoStream, subtitleStreams));

    public async Task SetAudioSourceAsync(IRandomAccessStream audioStream, bool preservePosition = false)
    {
        var previousPosition = Position;

        _audioFFmpegSource = await FFmpegMediaSource.CreateFromStreamAsync(audioStream, _config);
        _audioPlayer.Source = _audioFFmpegSource.CreateMediaPlaybackItem();

        // ToDo: move to separate class
        var smtc = _audioPlayer.SystemMediaTransportControls;
        smtc.DisplayUpdater.Type = MediaPlaybackType.Video;
        smtc.DisplayUpdater.Update();

        _audioPlayer.PlaybackSession.Position = previousPosition;

        LengthChanged?.Invoke(this, Length);
    }

    public async Task SetVideoSourceAsync(IRandomAccessStream videoStream, IEnumerable<IRandomAccessStream> subtitleStreams)
    {
        _videoFFmpegSource = await FFmpegMediaSource.CreateFromStreamAsync(videoStream, _config);
        
        foreach (var stream in subtitleStreams)
            await _videoFFmpegSource.AddExternalSubtitleAsync(stream);

        var a = _videoFFmpegSource.CreateMediaPlaybackItem();
        
        if (subtitleStreams.Any())
            a.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);

        _videoPlayer.Source = a;
    }

    // dispose resources on unload
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _syncTimer.Stop();

        _audioPlayer.Dispose();
        _videoPlayer.Dispose();

        _audioFFmpegSource?.Dispose();
        _videoFFmpegSource?.Dispose();
    }
}