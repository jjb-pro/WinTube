using Windows.UI.Xaml.Controls;
using WinTube.Model.Observable;
using WinTube.ViewModels;

namespace WinTube.Pages
{
    public sealed partial class SearchPage : Page
    {
        // ToDo: use DI
        public SearchViewModel ViewModel { get; } = new SearchViewModel();

        public SearchPage()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            Frame.Navigate(typeof(ViewPage), ((ObservableVideoSearchResult)listBox.SelectedItem).VideoId);
        }
    }
}