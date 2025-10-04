using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTube.TemplateSelectors;
using WinTube.ViewModels;

namespace WinTube.Pages;

#nullable enable

public sealed partial class SearchPage : Page
{
    public SearchViewModel ViewModel { get; } = ((App)Application.Current).Container.GetRequiredService<SearchViewModel>();

    public SearchPage()
    {
        InitializeComponent();

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += (s, e) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SearchViewModel.IsCompactMode))
            return;

        var selector = (SearchResultTemplateSelector)Resources["SearchResultTemplateSelector"];
        selector.UseCompact = ViewModel.IsCompactMode;

        var oldSelector = searchResultsListView.ItemTemplateSelector;
        searchResultsListView.ItemTemplateSelector = null;
        searchResultsListView.ItemTemplateSelector = oldSelector;
    }
}