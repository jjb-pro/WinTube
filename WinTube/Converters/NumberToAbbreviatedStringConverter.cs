using System;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters;

public class NumberToAbbreviatedStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || !long.TryParse(value.ToString(), out var n))
            return "0";
        else if (n >= 1000000000)
            return $"{n / 1000000000D:0.#}B";
        else if (n >= 1000000)
            return $"{n / 1000000D:0.#}M";
        else if (n >= 1000)
            return $"{n / 1000D:0.#}K";

        return n.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}