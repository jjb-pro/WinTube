using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public class ShortDateOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset offset)
            return offset.ToLocalTime().ToString("dd.MM.yy HH:mm");

        return "unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}