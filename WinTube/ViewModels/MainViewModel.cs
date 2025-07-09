using CommunityToolkit.Mvvm.Messaging;
using WinTube.Model;
using WinTube.Pages;
using WinTube.Services;
using WinTube.ViewModels;

namespace WinTube.ViewModels
{
    public partial class MainViewModel(NavigationService navigationService) : BindableBase
    {
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public void OnQuerySubmitted()
        {
            navigationService.NavigateTo(typeof(SearchPage));

            WeakReferenceMessenger.Default.Send(new SearchRequestedMessage(SearchText));
        }
    }
}