//using System;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;

//#nullable enable

//namespace WinTube.Controls;

//public sealed class Shimmer : Control
//{
//    //private const float InitialStartPointX = -7.92f;
//    //private const string PART_Shape = "Shape";

//    //private ExpressionAnimation? _sizeExpressionAnimation;
//    //private Vector2KeyFrameAnimation? _gradientStartPointAnimation;
//    //private Vector2KeyFrameAnimation? _gradientEndPointAnimation;
//    //private CompositionColorGradientStop? _gradientStop1;
//    //private CompositionColorGradientStop? _gradientStop2;
//    //private CompositionColorGradientStop? _gradientStop3;
//    //private CompositionColorGradientStop? _gradientStop4;
//    //private CompositionRoundedRectangleGeometry? _rectangleGeometry;
//    //private ShapeVisual? _shapeVisual;
//    //private CompositionLinearGradientBrush? _shimmerMaskGradient;
//    //private Border? _shape;

//    //private bool _initialized;
//    //private bool _animationStarted;

//    public Shimmer()
//    {
//        DefaultStyleKey = typeof(Shimmer);
//        //Loaded += OnLoaded;
//        //Unloaded += OnUnloaded;
//    }

//    #region DependencyProperties

//    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
//       nameof(Duration),
//       typeof(TimeSpan),
//       typeof(Shimmer),
//       new PropertyMetadata(TimeSpan.FromMilliseconds(1600), OnDependencyPropertyChanged));

//    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
//      nameof(IsActive),
//      typeof(bool),
//      typeof(Shimmer),
//      new PropertyMetadata(true, OnDependencyPropertyChanged));

//    public TimeSpan Duration
//    {
//        get => (TimeSpan)GetValue(DurationProperty);
//        set => SetValue(DurationProperty, value);
//    }

//    public bool IsActive
//    {
//        get => (bool)GetValue(IsActiveProperty);
//        set => SetValue(IsActiveProperty, value);
//    }

//    private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        //if (d is Shimmer s)
//        //{
//        //    if (s.IsActive)
//        //    {
//        //        s.StopAnimation();
//        //        s.TryStartAnimation();
//        //    }
//        //    else
//        //    {
//        //        s.StopAnimation();
//        //    }
//        //}
//    }

//    #endregion

//    //protected override void OnApplyTemplate()
//    //{
//    //    base.OnApplyTemplate();

//    //    _shape = GetTemplateChild(PART_Shape) as Border;

//    //    // If template applied and control loaded, init resources and start animation if active
//    //    if (!_initialized && TryInitializationResource() && IsActive)
//    //    {
//    //        TryStartAnimation();
//    //    }
//    //}

//    //private void OnLoaded(object sender, RoutedEventArgs e)
//    //{
//    //    if (!_initialized && TryInitializationResource() && IsActive)
//    //    {
//    //        TryStartAnimation();
//    //    }

//    //    ActualThemeChanged += OnActualThemeChanged;
//    //}

//    //private void OnUnloaded(object sender, RoutedEventArgs e)
//    //{
//    //    ActualThemeChanged -= OnActualThemeChanged;
//    //    StopAnimation();

//    //    if (_initialized && _shape != null)
//    //    {
//    //        ElementCompositionPreview.SetElementChildVisual(_shape, null);

//    //        // Dispose composition objects if available
//    //        _rectangleGeometry?.Dispose();
//    //        _shapeVisual?.Dispose();
//    //        _shimmerMaskGradient?.Dispose();
//    //        _gradientStop1?.Dispose();
//    //        _gradientStop2?.Dispose();
//    //        _gradientStop3?.Dispose();
//    //        _gradientStop4?.Dispose();

//    //        _rectangleGeometry = null;
//    //        _shapeVisual = null;
//    //        _shimmerMaskGradient = null;
//    //        _gradientStop1 = null;
//    //        _gradientStop2 = null;
//    //        _gradientStop3 = null;
//    //        _gradientStop4 = null;

//    //        _initialized = false;
//    //    }
//    //}

//    //private void OnActualThemeChanged(FrameworkElement sender, object args)
//    //{
//    //    if (!_initialized) return;
//    //    SetGradientStopColorsByTheme();
//    //}

//    //private bool TryInitializationResource()
//    //{
//    //    if (_initialized) return true;
//    //    if (_shape is null || !IsLoaded) return false;

//    //    // Use built-in API to get the visual for the shape element
//    //    var rootVisual = ElementCompositionPreview.GetElementVisual(_shape);
//    //    var compositor = rootVisual.Compositor;

//    //    // Create geometry/visual/brush/stop objects
//    //    _rectangleGeometry = compositor.CreateRoundedRectangleGeometry();
//    //    _shapeVisual = compositor.CreateShapeVisual();
//    //    _shimmerMaskGradient = compositor.CreateLinearGradientBrush();
//    //    _gradientStop1 = compositor.CreateColorGradientStop();
//    //    _gradientStop2 = compositor.CreateColorGradientStop();
//    //    _gradientStop3 = compositor.CreateColorGradientStop();
//    //    _gradientStop4 = compositor.CreateColorGradientStop();

//    //    // Configure gradient stops + brush
//    //    SetGradientAndStops();
//    //    SetGradientStopColorsByTheme();

//    //    // Set rectangle corner radius from Border's CornerRadius.TopLeft
//    //    var corner = (float)(_shape.CornerRadius.TopLeft);
//    //    _rectangleGeometry.CornerRadius = new Vector2(corner, corner);

//    //    var spriteShape = compositor.CreateSpriteShape(_rectangleGeometry);
//    //    spriteShape.FillBrush = _shimmerMaskGradient;
//    //    _shapeVisual.Shapes.Add(spriteShape);

//    //    ElementCompositionPreview.SetElementChildVisual(_shape, _shapeVisual);

//    //    _initialized = true;
//    //    return true;
//    //}

//    //private void SetGradientAndStops()
//    //{
//    //    _shimmerMaskGradient!.StartPoint = new Vector2(InitialStartPointX, 0.0f);
//    //    _shimmerMaskGradient.EndPoint = new Vector2(0.0f, 1.0f);

//    //    _gradientStop1!.Offset = 0.273f;
//    //    _gradientStop2!.Offset = 0.436f;
//    //    _gradientStop3!.Offset = 0.482f;
//    //    _gradientStop4!.Offset = 0.643f;

//    //    _shimmerMaskGradient.ColorStops.Add(_gradientStop1);
//    //    _shimmerMaskGradient.ColorStops.Add(_gradientStop2);
//    //    _shimmerMaskGradient.ColorStops.Add(_gradientStop3);
//    //    _shimmerMaskGradient.ColorStops.Add(_gradientStop4);
//    //}

//    //private void SetGradientStopColorsByTheme()
//    //{
//    //    // Slight opacity depending on theme to make a subtle shimmer
//    //    switch (ActualTheme)
//    //    {
//    //        case ElementTheme.Default:
//    //        case ElementTheme.Dark:
//    //            _gradientStop1!.Color = Color.FromArgb((byte)(255 * 0.0605), 255, 255, 255);
//    //            _gradientStop2!.Color = Color.FromArgb((byte)(255 * 0.0326), 255, 255, 255);
//    //            _gradientStop3!.Color = Color.FromArgb((byte)(255 * 0.0326), 255, 255, 255);
//    //            _gradientStop4!.Color = Color.FromArgb((byte)(255 * 0.0605), 255, 255, 255);
//    //            break;

//    //        case ElementTheme.Light:
//    //            _gradientStop1!.Color = Color.FromArgb((byte)(255 * 0.0537), 0, 0, 0);
//    //            _gradientStop2!.Color = Color.FromArgb((byte)(255 * 0.0289), 0, 0, 0);
//    //            _gradientStop3!.Color = Color.FromArgb((byte)(255 * 0.0289), 0, 0, 0);
//    //            _gradientStop4!.Color = Color.FromArgb((byte)(255 * 0.0537), 0, 0, 0);
//    //            break;
//    //    }
//    //}

//    //private void TryStartAnimation()
//    //{
//    //    if (_animationStarted || !_initialized || _shape is null || _shapeVisual is null || _rectangleGeometry is null)
//    //    {
//    //        return;
//    //    }

//    //    var rootVisual = ElementCompositionPreview.GetElementVisual(_shape);
//    //    var compositor = rootVisual.Compositor;

//    //    // Expression animation linking the Size of shapeVisual and geometry to the root visual size
//    //    var sizeExpression = compositor.CreateExpressionAnimation("hostVisual.Size");
//    //    sizeExpression.SetReferenceParameter("hostVisual", rootVisual);
//    //    _sizeExpressionAnimation = sizeExpression;

//    //    _shapeVisual.StartAnimation(nameof(ShapeVisual.Size), _sizeExpressionAnimation);
//    //    _rectangleGeometry.StartAnimation(nameof(CompositionRoundedRectangleGeometry.Size), _sizeExpressionAnimation);

//    //    // Gradient start point animation
//    //    _gradientStartPointAnimation = compositor.CreateVector2KeyFrameAnimation();
//    //    _gradientStartPointAnimation.Duration = Duration;
//    //    _gradientStartPointAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
//    //    _gradientStartPointAnimation.InsertKeyFrame(0.0f, new Vector2(InitialStartPointX, 0.0f));
//    //    _gradientStartPointAnimation.InsertKeyFrame(1.0f, Vector2.Zero);
//    //    _shimmerMaskGradient!.StartAnimation(nameof(CompositionLinearGradientBrush.StartPoint), _gradientStartPointAnimation);

//    //    // Gradient end point animation
//    //    _gradientEndPointAnimation = compositor.CreateVector2KeyFrameAnimation();
//    //    _gradientEndPointAnimation.Duration = Duration;
//    //    _gradientEndPointAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
//    //    _gradientEndPointAnimation.InsertKeyFrame(0.0f, new Vector2(1.0f, 0.0f));
//    //    _gradientEndPointAnimation.InsertKeyFrame(1.0f, new Vector2(-InitialStartPointX, 1.0f));
//    //    _shimmerMaskGradient.StartAnimation(nameof(CompositionLinearGradientBrush.EndPoint), _gradientEndPointAnimation);

//    //    _animationStarted = true;
//    //}

//    //private void StopAnimation()
//    //{
//    //    if (!_animationStarted) return;

//    //    _shapeVisual?.StopAnimation(nameof(ShapeVisual.Size));
//    //    _rectangleGeometry?.StopAnimation(nameof(CompositionRoundedRectangleGeometry.Size));
//    //    _shimmerMaskGradient?.StopAnimation(nameof(CompositionLinearGradientBrush.StartPoint));
//    //    _shimmerMaskGradient?.StopAnimation(nameof(CompositionLinearGradientBrush.EndPoint));

//    //    _gradientStartPointAnimation?.Dispose();
//    //    _gradientEndPointAnimation?.Dispose();

//    //    _sizeExpressionAnimation = null;
//    //    _animationStarted = false;
//    //}
//}