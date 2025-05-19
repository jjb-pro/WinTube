using Windows.UI.Xaml.Media.Imaging;
using WinTube.ViewModels;

namespace WinTube.Model.Observable
{
    public abstract class ObservableSearchResult : BindableBase
    {
        public abstract string Title { get; }

        public abstract WriteableBitmap Thumbnail { get; set; }

        public abstract string ChannelTitle { get; }
    }
}