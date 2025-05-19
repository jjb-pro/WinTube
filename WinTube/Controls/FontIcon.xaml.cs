using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinTube.Controls
{
    public sealed partial class FontIcon : UserControl
    {
        public FontIcon() => InitializeComponent();

        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(
                nameof(Glyph),
                typeof(string),
                typeof(FontIcon),
                new PropertyMetadata(string.Empty));
    }
}