using WinTube.Model.Observable;

namespace WinTube.ViewModels;

public class VideoSelectedMessage(ObservableVideoSearchResult selectedVideoSearchResult)
{
    public ObservableVideoSearchResult SelectedVideoSearchResult { get; } = selectedVideoSearchResult;
}