using AsyncAwaitBestPractices;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Universal.WebP;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace WinTube.Model.Observable
{
    public class ObservableVideoSearchResult : ObservableSearchResult
    {
        public VideoId VideoId { get; }

        public override string Title { get; }
        public TimeSpan? Duration { get; }

        private WriteableBitmap _thumbnail;
        public override WriteableBitmap Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public override string ChannelTitle { get; }

        public ObservableVideoSearchResult(VideoSearchResult videoSearchResult)
        {
            VideoId = videoSearchResult.Id;

            Title = videoSearchResult.Title;
            Duration = videoSearchResult.Duration;
            FetchThumbnail(videoSearchResult.Thumbnails.First().Url).SafeFireAndForget();

            ChannelTitle = videoSearchResult.Author.ChannelTitle;
        }

        private async Task FetchThumbnail(string url)
        {
            var bytes = await new HttpClient().GetByteArrayAsync(url);

            var webp = new WebPDecoder();
            var pixelData = (await webp.DecodeBgraAsync(bytes)).ToArray();

            var size = await webp.GetSizeAsync(bytes);
            var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);

            var stream = bitmap.PixelBuffer.AsStream();
            await stream.WriteAsync(pixelData, 0, pixelData.Length);

            Thumbnail = bitmap;
        }
    }
}