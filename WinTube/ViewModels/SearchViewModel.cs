#nullable enable
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;
using Windows.Networking.Connectivity;
using WinTube.Model;
using WinTube.Model.Observable;
using WinTube.Pages;
using WinTube.Services;
using YoutubeExplode;

namespace WinTube.ViewModels;

public partial class SearchViewModel : ObservableObject, IRecipient<SearchRequestedMessage>
{
    private readonly YoutubeClient _client;
    private readonly NavigationService _navigationService;

    [ObservableProperty] private bool _isProgressBarVisible;
    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private ObservableVideoSearchResult? _selectedVideo;

    [ObservableProperty] private IncrementalVideoSearchCollection? _searchResults;
    private CancellationTokenSource? _cts;

    public SearchViewModel(YoutubeClient youtubeClient, NavigationService navigationService)
    {
        _client = youtubeClient;
        _navigationService = navigationService;

        var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
        IsOffline = connectionProfile == null || connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None;
        SearchResults = null;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(SearchRequestedMessage message)
    {
        _cts?.Cancel();

        _cts = new();
        SearchResults = new IncrementalVideoSearchCollection(_client, message.Query, _cts.Token);
    }

    public void OnVideoSelected()
    {
        if (null == SelectedVideo)
            return;

        _navigationService.OpenOverlay(typeof(ViewPage));
        WeakReferenceMessenger.Default.Send(new VideoSelectedMessage(SelectedVideo));
    }
}