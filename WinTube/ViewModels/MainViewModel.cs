using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using WinTube.Model;
using WinTube.Pages;
using WinTube.Services;

#nullable enable

namespace WinTube.ViewModels;

public partial class MainViewModel(NavigationService navigationService, YoutubeSuggestionService suggestionService) : ObservableObject
{
    [ObservableProperty] private bool _isSearchBoxVisible;
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isTitleBarVisible = true;

    public ObservableCollection<SuggestionResult> Suggestions { get; } = [];

    [RelayCommand]
    public void OnToggleSearchBoxVisibility() => IsSearchBoxVisible = !IsSearchBoxVisible;

    private CancellationTokenSource? _typingCts;
    private CancellationTokenSource? _requestCts;
    public async void OnQueryChanged()
    {
        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(100, _typingCts.Token); // debounce delay
        }
        catch (TaskCanceledException)
        {
            return;
        }

        _requestCts?.Cancel();
        _requestCts = new CancellationTokenSource();

        try
        {
            UpdateSuggestions(await suggestionService.GetSuggestionsAsync(SearchText, _requestCts.Token));
        }
        catch (OperationCanceledException) { }
    }

    private void UpdateSuggestions(IEnumerable<SuggestionResult> newItems)
    {
        var newSet = new HashSet<SuggestionResult>(newItems);

        // Remove items that no longer exist
        for (int i = Suggestions.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(Suggestions[i]))
                Suggestions.RemoveAt(i);
        }

        // Add missing items (preserve newItems order)
        foreach (var item in newItems)
        {
            if (!Suggestions.Contains(item))
                Suggestions.Add(item);
        }
    }

    public void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        string query;

        if (args.ChosenSuggestion is SuggestionResult suggestion)
        {
            query = suggestion.Text;
            SearchText = suggestion.Text;
        }
        else
        {
            query = args.QueryText;
        }

        navigationService.NavigateTo(typeof(SearchPage));
        WeakReferenceMessenger.Default.Send(new SearchRequestedMessage(query));
    }
}