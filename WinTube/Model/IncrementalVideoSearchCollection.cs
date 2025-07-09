using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using WinTube.Model.Observable;
using YoutubeExplode.Search;
using YoutubeExplode;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;

namespace WinTube.Helpers;

public class IncrementalVideoSearchCollection : ObservableCollection<ObservableVideoSearchResult>, ISupportIncrementalLoading
{
    private readonly YoutubeClient _client;
    private readonly string _searchQuery;
    private readonly CancellationToken _cancellationToken;
    private IAsyncEnumerator<VideoSearchResult> _videoEnumerator;
    private bool _hasMoreItems = true;

    public IncrementalVideoSearchCollection(YoutubeClient client, string searchQuery, CancellationToken cancellationToken)
    {
        _client = client;
        _searchQuery = searchQuery;
        _cancellationToken = cancellationToken;
        _videoEnumerator = _client.Search.GetVideosAsync(_searchQuery, _cancellationToken).GetAsyncEnumerator();
    }

    public bool HasMoreItems => _hasMoreItems;

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return AsyncInfo.Run<LoadMoreItemsResult>(async (cancellationToken) =>
        {
            int loadedCount = 0;

            while (loadedCount < count)
            {
                bool hasNext = await _videoEnumerator
                                    .MoveNextAsync()
                                    .AsTask()
                                    .ConfigureAwait(false);

                if (!hasNext)
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

            return new LoadMoreItemsResult
            {
                Count = (uint)loadedCount
            };
        });
    }
}