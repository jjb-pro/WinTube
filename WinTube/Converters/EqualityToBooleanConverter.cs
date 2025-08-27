using System;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public class EqualityToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return false;

        if (double.TryParse(value.ToString(), out double val) && double.TryParse(parameter.ToString(), out double param))
            return val == param;

        return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}