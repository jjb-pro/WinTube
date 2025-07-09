using System;

namespace WinTube.Model;

public class NamedCaptionSource(string name, Uri uri) : NamedSource(name)
{
    public readonly Uri Uri = uri;
}