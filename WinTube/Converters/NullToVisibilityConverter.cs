using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public sealed class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isNullOrEmpty = value == null;

        if (value is string str)
            isNullOrEmpty = string.IsNullOrWhiteSpace(str);

        if (Invert)
            isNullOrEmpty = !isNullOrEmpty;

        return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}