using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinTube.Model;

#nullable enable

namespace WinTube.Services;

public class YoutubeSuggestionService
{
    private readonly HttpClient _client = new();
    private readonly List<(string Query, IReadOnlyList<SuggestionResult> Results)> _cache = [];
    private const int MaxEntries = 20;

    public YoutubeSuggestionService()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        _client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        _client.DefaultRequestHeaders.Referrer = new Uri("https://www.youtube.com/");
    }

    public async Task<IEnumerable<SuggestionResult>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        var (_, results) = _cache.Find(x => x.Query == query);
        if (results is not null)
            return results;

        results = await FetchSuggestionsAsync(query, cancellationToken);

        if (_cache.Count == MaxEntries)
            _cache.RemoveAt(0);

        _cache.Add((query, results));
        return results;
    }

    private async Task<IReadOnlyList<SuggestionResult>> FetchSuggestionsAsync(string query, CancellationToken cancellationToken)
    {
        var encoded = UrlEncoder.Default.Encode(query);
        var uri = $"https://suggestqueries-clients6.youtube.com/complete/search?ds=yt&client=youtube&q={encoded}";

        using var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var start = body.IndexOf('[');
        var end = body.LastIndexOf(']');

        if (start < 0 || end <= start)
            return [];

        var json = body.Substring(start, end - start + 1);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 2)
            return [];

        var suggestions = root[1];
        if (suggestions.ValueKind != JsonValueKind.Array)
            return [];

        var results = new List<SuggestionResult>();

        foreach (var item in suggestions.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Array || item.GetArrayLength() == 0)
                continue;

            var text = item[0].GetString() ?? string.Empty;
            string? thumb = null;

            if (item.GetArrayLength() > 3 && item[3].ValueKind == JsonValueKind.Object)
            {
                var meta = item[3];
                if (meta.TryGetProperty("zai", out var zai) && zai.ValueKind == JsonValueKind.String)
                {
                    thumb = zai.GetString();
                }
                else if (meta.TryGetProperty("zal", out var zal) && zal.ValueKind == JsonValueKind.String)
                {
                    var id = zal.GetString();
                    if (!string.IsNullOrEmpty(id))
                        thumb = $"https://i.ytimg.com/vi/{id}/mqdefault.jpg";
                }
            }

            results.Add(new SuggestionResult(text, thumb));
        }

        return results;
    }
}