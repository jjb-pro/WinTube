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

    Task<IRandomAccessStream> GetStreamAsync();
}

public class NamedUriStreamSource(string name, Uri uri) : INamedStreamSource
{
    public string Name { get; } = name;

    public async Task<IRandomAccessStream> GetStreamAsync()
    {
        using var http = new HttpClient();

        var bytes = await http.GetByteArrayAsync(uri);

        var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(bytes.AsBuffer());
        stream.Seek(0);

        return stream;
    }

    public override string ToString() => Name;
}

public class NamedYouTubeStreamSource<T>(string name, YoutubeClient client, T streamInfo) : INamedStreamSource where T : IStreamInfo
{
    public string Name { get; } = name;

    public async Task<IRandomAccessStream> GetStreamAsync() => (await client.Videos.Streams.GetAsync(streamInfo)).AsRandomAccessStream();

    public override string ToString() => Name;
}