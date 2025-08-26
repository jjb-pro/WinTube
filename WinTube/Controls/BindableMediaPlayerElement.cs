#nullable enable
using FFmpegInteropX;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinTube.Controls;

public class BindableMediaPlayerElement : MediaPlayerElement
{
    public static readonly DependencyProperty StreamSourceProperty =
        DependencyProperty.Register(
            nameof(StreamSource),
            typeof(Uri),
            typeof(BindableMediaPlayerElement),
            new PropertyMetadata(null, OnStreamSourceChanged));

    public Uri? StreamSource
    {
        get => (Uri?)GetValue(StreamSourceProperty);
        set => SetValue(StreamSourceProperty, value);
    }

    private static async void OnStreamSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not BindableMediaPlayerElement playerElement)
            return;

        if (e.NewValue is Uri uri)
            playerElement.Source = await CreateMediaSource(uri);
        else
            playerElement.Source = null;
    }

    private static async Task<MediaPlaybackItem> CreateMediaSource(Uri uri)
    {
        var config = new MediaSourceConfig
        {
            MaxVideoThreads = 4,
            SkipErrors = uint.MaxValue,
            FastSeek = true,
            FFmpegOptions = new PropertySet {
                        { "reconnect", 1 },
                        { "reconnect_streamed", 1 },
                        { "reconnect_on_network_error", 1 }
                    }
        };

        var ffmpegMediaSource = await FFmpegMediaSource.CreateFromUriAsync(uri.ToString(), config);
        return ffmpegMediaSource.CreateMediaPlaybackItem();
    }
}