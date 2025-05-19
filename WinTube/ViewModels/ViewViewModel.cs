using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace WinTube.ViewModels
{
    public class ViewViewModel : BindableBase
    {
        // ToDo: use DI
        private readonly YoutubeClient _client = new YoutubeClient();

        private MediaSource _audioMediaSource;
        public MediaSource AudioMediaSource
        {
            get => _audioMediaSource;
            set => SetProperty(ref _audioMediaSource, value);
        }

        private MediaSource _videoMediaSource;
        public MediaSource VideoMediaSource
        {
            get => _videoMediaSource;
            set => SetProperty(ref _videoMediaSource, value);
        }


        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }


        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private TimeSpan? _duration;
        public TimeSpan? Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public async Task Watch(VideoId videoId)
        {
            var video = await _client.Videos.GetAsync(videoId);
            Title = video.Title;
            Duration = video.Duration;

            var streamManifest = await _client.Videos.Streams.GetManifestAsync(videoId);
            {
                var streamInfo = streamManifest
                .GetAudioStreams()
                    .Where(x => x.IsAudioLanguageDefault == true)
                    .OrderBy(x => x.Bitrate)
                    .Last();
                AudioMediaSource = MediaSource.CreateFromStream((await _client.Videos.Streams.GetAsync(streamInfo)).AsRandomAccessStream(), "audio/mp3");
            }
            {
                var streamInfos = streamManifest
                    .GetVideoStreams()
                    .Where(x => x.Container == Container.Mp4)
                    .OrderBy(x => x.VideoResolution.Area)
                    .ToList();
                var streamInfo = streamInfos[streamInfos.Count - 2];

                VideoMediaSource = MediaSource.CreateFromStream((await _client.Videos.Streams.GetAsync(streamInfo)).AsRandomAccessStream(), "video/mp4");

                //var trackManifest = await _client.Videos.ClosedCaptions.GetManifestAsync(videoId);
                //foreach (var track in trackManifest.Tracks)
                //    VideoMediaSource.ExternalTimedTextSources.Add(TimedTextSource.CreateFromUri(new Uri(track.Url), track.Language.Name));
            }

            IsPlaying = true;
        }
    }
}