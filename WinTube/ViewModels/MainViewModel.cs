using System;
using System.Windows.Input;
using WinTube.Pages;
using WinTube.Services;
using WinTube.ViewModels;

namespace Mail.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly NavigationService _navigationService;

        public ICommand NavigateBackCommand { get; }
        public ICommand NavigateHomeCommand { get; }
        public ICommand NavigateSearchCommand { get; }
        public ICommand NavigateSettingsCommand { get; }

        public MainViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;

            //BackCommand = new RelayCommand();
            NavigateBackCommand = new RelayCommand(GoBack, () => _navigationService.CanGoBack);
            NavigateHomeCommand = new RelayCommand(() => NavigateTo(typeof(HomePage)));
            NavigateSearchCommand = new RelayCommand(() => NavigateTo(typeof(SearchPage)));
            NavigateSettingsCommand = new RelayCommand(() => NavigateTo(typeof(SettingsPage)));
        }

        private void GoBack()
        {
            _navigationService.GoBack();
            ((RelayCommand)NavigateBackCommand).RaiseCanExecuteChanged();
        }

        private void NavigateTo(Type pageType)
        {
            _navigationService.NavigateTo(pageType);
            ((RelayCommand)NavigateBackCommand).RaiseCanExecuteChanged();
        }
    }
}