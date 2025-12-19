#nullable enable

namespace WinTube.Model;

public record SuggestionResult(string Text, string? ThumbnailUrl)
{
    public override string ToString() => Text;
}