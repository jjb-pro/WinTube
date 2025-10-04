#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTube.Model;
using WinTube.ViewModels;

namespace WinTube.Pages
{
    public sealed partial class ViewPage : Page
    {
        private DataTransferManager _dataTransferManager;
        private ShareRequest? _shareData;

        public ViewViewModel ViewModel { get; } = ((App)Application.Current).Container.GetService<ViewViewModel>();

        public ViewPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += OnDataRequested;

            ViewModel.ShareRequested += OnShareRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _dataTransferManager.DataRequested -= OnDataRequested;
            ViewModel.ShareRequested -= OnShareRequested;
        }

        private void OnShareRequested(object sender, ShareRequest e)
        {
            _shareData = e;
            DataTransferManager.ShowShareUI();
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (null == _shareData || null == _shareData.Uri)
                return;

            var request = args.Request;
            request.Data.Properties.Title = "Share " + _shareData.Title;
            request.Data.Properties.Description = _shareData.Description;
            request.Data.SetWebLink(_shareData.Uri);
        }
    }
}