using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace WinTube.Converters
{
    public class PlayBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isPaused)
                return new SymbolIcon(isPaused ? Symbol.Pause : Symbol.Play);
            
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}