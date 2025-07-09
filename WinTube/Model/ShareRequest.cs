using System;

namespace WinTube.Model; 

public class ShareRequest(string title, string description, Uri uri)
{
    public readonly string Title = title;
    public readonly string Description = description;
    public readonly Uri Uri = uri;
}