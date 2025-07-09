using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not bool booleanValue)
            return Visibility.Visible;

        if (bool.TryParse((string)parameter, out var inverse) && inverse)
            return booleanValue ? Visibility.Collapsed : Visibility.Visible;
        else
            return booleanValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}