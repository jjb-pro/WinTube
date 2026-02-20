using DependencyPropertyGenerator;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WinTube.Controls;

public class SeekRequestedEventArgs(TimeSpan position) : EventArgs
{
    public TimeSpan Position { get; } = position;
}

[DependencyProperty<TimeSpan>("Position")]
[DependencyProperty<TimeSpan>("Length")]
public sealed partial class SeekBar : UserControl
{
    public event EventHandler<SeekRequestedEventArgs> SeekRequested;

    public SeekBar() => InitializeComponent();

    partial void OnPositionChanged() => UpdateVisuals();

    private void UpdateVisuals()
    {
        if (Length.TotalMilliseconds <= 0 || _isDragging)
            return;

        var progress = Position.TotalMilliseconds / Length.TotalMilliseconds;
        var trackWidth = TrackBackground.ActualWidth;

        var fillWidth = progress * trackWidth;

        TrackFill.Width = fillWidth;
        Canvas.SetLeft(Thumb, fillWidth - Thumb.Width / 2);
    }

    // dragging logic
    private bool _isDragging;
    private double _dragX;

    private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
        Thumb.CapturePointer(e.Pointer);
    }

    private void Thumb_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging)
            return;

        var point = e.GetCurrentPoint(TrackBackground).Position;
        _dragX = Math.Max(0, Math.Min(point.X, TrackBackground.ActualWidth));

        TrackFill.Width = _dragX;
        Canvas.SetLeft(Thumb, _dragX - Thumb.Width / 2);
    }

    private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        Thumb.ReleasePointerCapture(e.Pointer);

        if (Length.TotalMilliseconds <= 0)
            return;

        double ratio = _dragX / TrackBackground.ActualWidth;
        Position = TimeSpan.FromMilliseconds(Length.TotalMilliseconds * ratio);

        SeekRequested?.Invoke(this, new(Position));
    }

    private void Track_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (Length.TotalMilliseconds <= 0) return;

        var point = e.GetCurrentPoint(TrackBackground).Position;
        UpdateFromX(point.X);
    }

    private void UpdateFromX(double x)
    {
        var width = TrackBackground.ActualWidth;
        x = Math.Max(0, Math.Min(x, width));

        var ratio = x / width;
        var position = TimeSpan.FromMilliseconds(Length.TotalMilliseconds * ratio);

        Position = position;
        SeekRequested?.Invoke(this, new(position));
    }
}