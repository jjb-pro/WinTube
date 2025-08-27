using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
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

public partial class ObservableVideoSearchResult : ObservableSearchResult
{
    private static readonly HttpClient _thumbnailClient = new();

    public ObservableVideoSearchResult(YoutubeClient client, VideoSearchResult videoSearchResult)
    {
        VideoId = videoSearchResult.Id;
        Title = videoSearchResult.Title;
        ChannelTitle = videoSearchResult.Author.ChannelTitle;
        Duration = videoSearchResult.Duration;

        FetchThumbnail(videoSearchResult.Thumbnails.OrderByDescending(t => t.Resolution.Area).First().Url).SafeFireAndForget();
        FetchChannelPicture(client, videoSearchResult.Author.ChannelId).SafeFireAndForget();
    }

    public VideoId VideoId { get; }

    public override string Title { get; }
    public override string ChannelTitle { get; }

    public TimeSpan? Duration { get; }

    [ObservableProperty] private WriteableBitmap _thumbnail;
    [ObservableProperty] private BitmapImage _channelPicture;

    private async Task FetchChannelPicture(YoutubeClient client, ChannelId id)
    {
        var channel = await client.Channels.GetAsync(id);
        ChannelPicture = new BitmapImage(new Uri(channel.Thumbnails.OrderBy(t => t.Resolution.Area).First().Url));
    }

    private async Task FetchThumbnail(string url)
    {
        var bytes = await _thumbnailClient.GetByteArrayAsync(url);

        try
        {
            var webp = new WebPDecoder();
            var size = await webp.GetSizeAsync(bytes);
            var pixelData = (await webp.DecodeBgraAsync(bytes)).ToArray();

            var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);
            using var stream = bitmap.PixelBuffer.AsStream();
            await stream.WriteAsync(pixelData, 0, pixelData.Length);

            Thumbnail = bitmap;
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Failed to decode WebP image: {ex.Message}");
        }
    }
}