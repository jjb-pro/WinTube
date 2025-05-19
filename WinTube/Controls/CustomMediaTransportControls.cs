using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WinTube.Controls
{
    public sealed class CustomMediaTransportControls : MediaTransportControls
    {
        public event EventHandler<RoutedEventArgs> PlayPauseButtonClicked;

        private Slider _progressSlider;
        private bool _isSliderDragged;
        public event EventHandler<double> ProgressSliderValueChanged;

        private ProgressBar _bufferingProgressBar;

        public CustomMediaTransportControls()
        {
            DefaultStyleKey = typeof(CustomMediaTransportControls);
        }

        private void SetSliderValue(double value)
        {

        }

        public void SetBufferingState(bool completed) => _bufferingProgressBar.Visibility = completed ? Visibility.Collapsed : Visibility.Visible;

        protected override void OnApplyTemplate()
        {
            var playPauseButton = GetTemplateChild("NewPlayPauseButton") as Button;
            playPauseButton.Click += (s, e) => PlayPauseButtonClicked?.Invoke(s, e);

            _progressSlider = GetTemplateChild("NewProgressSlider") as Slider;
            _progressSlider.ManipulationMode = ManipulationModes.TranslateX;
            _progressSlider.ManipulationStarted += (s, e) =>
            {
                _isSliderDragged = true;
            };
            _progressSlider.ManipulationCompleted += (s, e) =>
            {
                ProgressSliderValueChanged?.Invoke(s, ((Slider)s).Value);
                _isSliderDragged = false;
            };

            _bufferingProgressBar = GetTemplateChild("NewBufferingProgressBar") as ProgressBar;

            base.OnApplyTemplate();
        }
    }
}