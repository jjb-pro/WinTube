using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using WinTube.Model.Observable;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace WinTube.ViewModels
{
    public class SearchViewModel : BindableBase
    {
        // ToDo: use DI
        private readonly YoutubeClient _client = new YoutubeClient();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _isProgressBarVisible;
        public bool IsProgressBarVisible
        {
            get => _isProgressBarVisible;
            set => SetProperty(ref _isProgressBarVisible, value);
        }

        private bool _isOffline;
        public bool IsOffline
        {
            get => _isOffline;
            set => SetProperty(ref _isOffline, value);
        }

        public ObservableCollection<ObservableSearchResult> SearchResults { get; } = new ObservableCollection<ObservableSearchResult>();

        private Task _searchTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object _lock = new object();

        public SearchViewModel()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            IsOffline = connectionProfile == null || connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.None;
        }

        public async void OnQuerySubmitted()
        {
            Task taskToAwait = null;

            lock (_lock)
            {
                if (_searchTask != null && !_searchTask.IsCompleted)
                {
                    _cancellationTokenSource.Cancel();
                    taskToAwait = _searchTask;
                }
            }

            if (taskToAwait != null)
                await taskToAwait;

            lock (_lock)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                _searchTask = SearchAsync();
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsProgressBarVisible = true;
                SearchResults.Clear();

                //var playlistResults = await _client.Search.GetPlaylistsAsync(_searchText, _cancellationTokenSource.Token).CollectAsync(10);
                //foreach (var searchResult in playlistResults)
                //    SearchResults.Add(new ObservablePlaylistSearchResult(searchResult));

                var videoResults = await _client.Search.GetVideosAsync(_searchText, _cancellationTokenSource.Token).CollectAsync(10);
                foreach (var searchResult in videoResults)
                    SearchResults.Add(new ObservableVideoSearchResult(searchResult));

                IsOffline = false;
            }
            catch (OperationCanceledException) { }
            catch (Exception)
            {
                IsOffline = true;
            }
            finally
            {
                IsProgressBarVisible = false;
            }
        }
    }
}