using DependencyPropertyGenerator;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace WinTube.Controls;

[DependencyProperty<CornerRadius>("Radius")]
public sealed partial class Shimmer : UserControl
{
    private Visual? _rootVisual;
    private Compositor? _compositor;
    private CompositionRoundedRectangleGeometry? _roundedGeometry;
    private CompositionGeometricClip? _geoClip;

    public Shimmer()
    {
        InitializeComponent();
        //Loaded += Shimmer_Loaded;
        //SizeChanged += Shimmer_SizeChanged;
    }

    //private void Shimmer_Loaded(object? sender, RoutedEventArgs e)
    //{
    //    _rootVisual = ElementCompositionPreview.GetElementVisual(RootGrid);
    //    _compositor = _rootVisual.Compositor;

    //    _roundedGeometry = _compositor.CreateRoundedRectangleGeometry();
    //    _geoClip = _compositor.CreateGeometricClip(_roundedGeometry);
    //    _rootVisual.Clip = _geoClip;

    //    UpdateClipSize();

    //    var storyboard = (Storyboard)RootGrid.Resources["ShimmerStoryboard"];
    //    storyboard.Begin();
    //}

    //private void Shimmer_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateClipSize();

    //private void UpdateClipSize()
    //{
    //    if (_roundedGeometry == null) return;
    //    var w = (float)RootGrid.ActualWidth;
    //    var h = (float)RootGrid.ActualHeight;
    //    _roundedGeometry.Size = new Vector2(w, h);

    //    var r = (float)Radius.TopLeft;
    //    _roundedGeometry.CornerRadius = new Vector2(r, r);
    //}

    //partial void OnRadiusChanged(CornerRadius newValue)
    //{
    //    if (_roundedGeometry == null) return;
    //    var r = (float)newValue.TopLeft;
    //    _roundedGeometry.CornerRadius = new Vector2(r, r);
    //}
}