namespace WinTube.Model;

public class NamedMediaSource(string name, string containerType, string url)
{
    public readonly string ContainerType = containerType;
    public readonly string Url = url;

    public override string ToString() => name;
}