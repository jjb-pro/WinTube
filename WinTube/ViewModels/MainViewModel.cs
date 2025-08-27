using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WinTube.Model;
using WinTube.Pages;
using WinTube.Services;

namespace WinTube.ViewModels;

public partial class MainViewModel(NavigationService navigationService) : ObservableObject
{
    [ObservableProperty] private bool _isSearchBoxVisible;
    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isTitleBarVisible = true;

    [RelayCommand]
    public void OnToggleSearchBoxVisibility() => IsSearchBoxVisible = !IsSearchBoxVisible;

    public void OnQuerySubmitted()
    {
        navigationService.NavigateTo(typeof(SearchPage));
        WeakReferenceMessenger.Default.Send(new SearchRequestedMessage(SearchText));
    }
}