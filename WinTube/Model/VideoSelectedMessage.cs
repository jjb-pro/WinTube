using YoutubeExplode.Videos;

namespace WinTube.ViewModels;

public class VideoSelectedMessage(VideoId videoId)
{
    public VideoId VideoId { get; } = videoId;
}