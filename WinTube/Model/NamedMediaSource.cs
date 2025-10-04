using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WinTube.Model;

public class NamedMediaSource(string name, string containerType, Func<Task<IRandomAccessStream>> getStreamCallback) : NamedSource(name)
{
    public readonly string ContainerType = containerType;

    public readonly Func<Task<IRandomAccessStream>> GetStreamCallback = getStreamCallback;
}

public abstract class NamedSource(string name)
{
    public override string ToString() => name;
}