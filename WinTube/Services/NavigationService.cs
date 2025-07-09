using System;
using Windows.UI.Xaml.Controls;
using WinTube.ViewModels;

namespace WinTube.Services
{
    public class NavigationService : BindableBase
    {
        private Frame _contentFrame;

        public bool CanGoBack => _contentFrame.CanGoBack;

        public void Initialize(Frame contentFrame) => _contentFrame = contentFrame;

        public void NavigateTo(Type pageType, object parameter = null)
        {
            if (_contentFrame.SourcePageType != pageType)
                _contentFrame.Navigate(pageType, parameter);
        }

        public void GoBack()
        {
            if (_contentFrame.CanGoBack)
                _contentFrame.GoBack();
        }
    }
}