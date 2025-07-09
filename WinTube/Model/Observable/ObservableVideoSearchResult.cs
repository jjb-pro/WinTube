using AsyncAwaitBestPractices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Universal.WebP;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace WinTube.Model.Observable;

public class ObservableVideoSearchResult : ObservableSearchResult
{
    private static readonly HttpClient _thumbnailClient = new();

    public VideoId VideoId { get; }

    public override string Title { get; }
    public override string ChannelTitle { get; }

    public TimeSpan? Duration { get; }

    private WriteableBitmap _thumbnail;
    public override WriteableBitmap Thumbnail
    {
        get => _thumbnail;
        set => SetProperty(ref _thumbnail, value);
    }

    private BitmapImage _channelPicture;
    public override BitmapImage ChannelPicture
    {
        get => _channelPicture;
        set => SetProperty(ref _channelPicture, value);
    }

    public ObservableVideoSearchResult(YoutubeClient client, VideoSearchResult videoSearchResult)
    {
        VideoId = videoSearchResult.Id;

        Title = videoSearchResult.Title;
        ChannelTitle = videoSearchResult.Author.ChannelTitle;

        Duration = videoSearchResult.Duration;

        FetchThumbnail(videoSearchResult.Thumbnails.First().Url).SafeFireAndForget();
        FetchChannelPicture(client, videoSearchResult.Author.ChannelId).SafeFireAndForget();
    }

    private async Task FetchChannelPicture(YoutubeClient client, ChannelId id)
    {
        var channel = await client.Channels.GetAsync(id);
        ChannelPicture = new BitmapImage(new Uri(channel.Thumbnails.OrderByDescending(t => t.Resolution.Area).First().Url));
    }

    private async Task FetchThumbnail(string url)
    {
        var bytes = await _thumbnailClient.GetByteArrayAsync(url);

        var webp = new WebPDecoder();
        var pixelData = (await webp.DecodeBgraAsync(bytes)).ToArray();

        var size = await webp.GetSizeAsync(bytes);
        var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);

        var stream = bitmap.PixelBuffer.AsStream();
        await stream.WriteAsync(pixelData, 0, pixelData.Length);

        Thumbnail = bitmap;
    }
}