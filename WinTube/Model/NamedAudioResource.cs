using AngleSharp.Dom;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace WinTube.Model;

public interface INamedStreamSource
{
    string Name { get; }
    Uri Uri { get; }
}

public class NamedUriStreamSource(string name, Uri uri) : INamedStreamSource
{
    public string Name { get; } = name;
    public Uri Uri { get; } = uri;

    public override string ToString() => Name;
}

public class NamedYouTubeStreamSource<T>(string name, T streamInfo) : INamedStreamSource where T : IStreamInfo
{
    public string Name { get; } = name;
    public Uri Uri { get; } = new Uri(streamInfo.Url);

    public override string ToString() => Name;
}