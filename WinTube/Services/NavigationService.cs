using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WinTube.Services;

public class NavigationService
{
    private Frame _contentFrame;
    private Frame _overlayFrame;

    public bool CanGoBack => _contentFrame.CanGoBack || null != _overlayFrame.Content;

    public NavigationService()
    {
        SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
    }

    public void Initialize(Frame contentFrame, Frame overlayFrame)
    {
        _contentFrame = contentFrame;
        _overlayFrame = overlayFrame;
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        if (!CanGoBack)
            return;

        GoBack();
        e.Handled = true;
    }

    public void NavigateTo(Type pageType, object parameter = null)
    {
        if (_contentFrame.SourcePageType != pageType)
            _contentFrame.Navigate(pageType, parameter);
    }

    public void OpenOverlay(Type pageType, object parameter = null) => _overlayFrame.Navigate(pageType, parameter);

    public void GoBack()
    {
        // first close overlay if exists
        if (null != _overlayFrame.Content)
            _overlayFrame.Content = null;
        else if (_contentFrame.CanGoBack)
            _contentFrame.GoBack();
    }
}