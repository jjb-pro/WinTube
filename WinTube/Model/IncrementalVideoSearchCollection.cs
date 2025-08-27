using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using WinTube.Model.Observable;
using YoutubeExplode;
using YoutubeExplode.Search;

public class IncrementalVideoSearchCollection : ObservableCollection<ObservableVideoSearchResult>, ISupportIncrementalLoading
{
    private readonly YoutubeClient _client;
    private readonly CancellationToken _cancellationToken;
    private readonly IAsyncEnumerator<VideoSearchResult> _videoEnumerator;
    private bool _hasMoreItems = true;
    private bool _isLoading = false;

    public IncrementalVideoSearchCollection(YoutubeClient client, string searchQuery, CancellationToken cancellationToken)
    {
        _client = client;
        _cancellationToken = cancellationToken;
        _videoEnumerator = _client.Search.GetVideosAsync(searchQuery, _cancellationToken).GetAsyncEnumerator();
    }

    public bool HasMoreItems => _hasMoreItems;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => AsyncInfo.Run(async ct =>
    {
        if (_isLoading)
            return new LoadMoreItemsResult { Count = 0 };

        _isLoading = true;
        int loadedCount = 0;

        try
        {
            while (loadedCount < count && !_cancellationToken.IsCancellationRequested)
            {
                if (!await _videoEnumerator.MoveNextAsync().AsTask())
                {
                    _hasMoreItems = false;
                    break;
                }

                var video = _videoEnumerator.Current;
                await Windows.ApplicationModel.Core.CoreApplication
                    .MainView
                    .CoreWindow
                    .Dispatcher
                    .RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Add(new ObservableVideoSearchResult(_client, video));
                    });

                loadedCount++;
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _isLoading = false;
        }

        return new LoadMoreItemsResult { Count = (uint)loadedCount };
    });
}