using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public class HalfValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double d)
            return new CornerRadius(d / 2.0);
        if (value is float f)
            return new CornerRadius(f / 2.0);
        if (value is int i)
            return new CornerRadius(i / 2.0);

        return new CornerRadius(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}