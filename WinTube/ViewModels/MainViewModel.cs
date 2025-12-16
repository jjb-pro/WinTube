using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
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

    public async void OnQueryChanged()
    {
        // ToDo: cancel last task
        Suggestions = await suggestionService.GetSuggestionsAsync(SearchText);
    }

    public void OnQuerySubmitted()
    {
        navigationService.NavigateTo(typeof(SearchPage));
        WeakReferenceMessenger.Default.Send(new SearchRequestedMessage(SearchText));
    }
}