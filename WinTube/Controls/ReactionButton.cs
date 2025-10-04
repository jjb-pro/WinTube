using DependencyPropertyGenerator;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace WinTube.Controls;

[DependencyProperty<string>("IconGlyph")]
[DependencyProperty<string>("Description")]
[DependencyProperty<ICommand>("Command")]
public sealed partial class ReactionButton : Control
{
    public ReactionButton() => DefaultStyleKey = typeof(ReactionButton);
}