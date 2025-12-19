using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Threading;
using WinTube.Model;
using WinTube.Pages;
using WinTube.Services;

namespace WinTube.ViewModels;

public partial class MainViewModel(NavigationService navigationService, YoutubeSuggestionService suggestionService) : ObservableObject
{
    [ObservableProperty] private bool _isSearchBoxVisible;
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isTitleBarVisible = true;

    [ObservableProperty] private IEnumerable<SuggestionResult> _suggestions = [];

    [RelayCommand]
    public void OnToggleSearchBoxVisibility() => IsSearchBoxVisible = !IsSearchBoxVisible;

    private CancellationTokenSource _cts;
    public async void OnQueryChanged()
    {
        // ToDo: cancel last task
        _cts?.Cancel();
        _cts = new();
        try
        {
            Suggestions = await suggestionService.GetSuggestionsAsync(SearchText, _cts.Token);
        } catch { }
    }

    public void OnQuerySubmitted()
    {
        navigationService.NavigateTo(typeof(SearchPage));
        WeakReferenceMessenger.Default.Send(new SearchRequestedMessage(SearchText));
    }
}