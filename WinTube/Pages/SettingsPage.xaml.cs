using System.Linq;
using Windows.UI.Xaml.Controls;

namespace WinTube.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage() => InitializeComponent();

        private void NumericTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) => sender.Text = new string(sender.Text.Where(char.IsDigit).ToArray());
    }
}