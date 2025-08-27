using CommunityToolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace WinTube.Model.Observable;

public abstract class ObservableSearchResult : ObservableObject
{
    public abstract string Title { get; }
    public abstract string ChannelTitle { get; }

    //public abstract WriteableBitmap Thumbnail { get; protected set; }
    //public abstract BitmapImage ChannelPicture { get; protected set; }
}