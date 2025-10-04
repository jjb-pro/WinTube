using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinTube.TemplateSelectors;

public class SearchResultTemplateSelector : DataTemplateSelector
{
    public DataTemplate CompactTemplate { get; set; }
    public DataTemplate DefaultTemplate { get; set; }

    public bool UseCompact { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => UseCompact ? CompactTemplate : DefaultTemplate;
}