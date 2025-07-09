using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinTube.Model;

namespace WinTube.Controls;

public sealed class CustomMediaTransportControls : MediaTransportControls
{
    public event EventHandler<RoutedEventArgs> PlayPauseButtonClicked;

    private Slider _progressSlider;
    private bool _isSliderDragged;
    public event EventHandler<double> ProgressSliderValueChanged;

    private ProgressBar _bufferingProgressBar;

    private ComboBox _audioStreamsComboBox;
    private ComboBox _videoStreamsComboBox;

    private AppBarToggleButton _ccSelectionButton;
    private ComboBox _captionsComboBox;

    // audio sources combo box
    public ObservableCollection<NamedMediaSource> AudioStreams
    {
        get => (ObservableCollection<NamedMediaSource>)GetValue(AudioStreamsProperty);
        set => SetValue(AudioStreamsProperty, value);
    }

    public static readonly DependencyProperty AudioStreamsProperty =
        DependencyProperty.Register(
            nameof(AudioStreams),
            typeof(ObservableCollection<NamedMediaSource>),
            typeof(SynchronizedMediaControl),
            new PropertyMetadata(new ObservableCollection<NamedMediaSource>()));

    public event EventHandler<NamedMediaSource> AudioStreamChanged;

    // video sources combo box
    public ObservableCollection<NamedMediaSource> VideoStreams
    {
        get => (ObservableCollection<NamedMediaSource>)GetValue(VideoStreamsProperty);
        set => SetValue(VideoStreamsProperty, value);
    }

    public static readonly DependencyProperty VideoStreamsProperty =
        DependencyProperty.Register(
            nameof(VideoStreams),
            typeof(ObservableCollection<NamedMediaSource>),
            typeof(SynchronizedMediaControl),
            new PropertyMetadata(new ObservableCollection<NamedMediaSource>()));

    public event EventHandler<NamedMediaSource> VideoStreamChanged;

    // subtitle sources combo box
    public ObservableCollection<NamedCaptionSource> CaptionSources
    {
        get => (ObservableCollection<NamedCaptionSource>)GetValue(CaptionSourcesProperty);
        set => SetValue(CaptionSourcesProperty, value);
    }

    public static readonly DependencyProperty CaptionSourcesProperty =
        DependencyProperty.Register(
            nameof(CaptionSources),
            typeof(ObservableCollection<NamedCaptionSource>),
            typeof(SynchronizedMediaControl),
            new PropertyMetadata(new ObservableCollection<NamedCaptionSource>()));

    public event EventHandler<int> CaptionChanged;

    public CustomMediaTransportControls()
    {
        DefaultStyleKey = typeof(CustomMediaTransportControls);
    }

    public void SetBufferingState(bool completed) => _bufferingProgressBar.Visibility = completed ? Visibility.Collapsed : Visibility.Visible;

    public void SetProgressSliderPosition(double percentage)
    {
        if (!_isSliderDragged)
            _progressSlider.Value = percentage;
    }

    protected override void OnApplyTemplate()
    {
        var playPauseButton = GetTemplateChild("NewPlayPauseButton") as Button;
        playPauseButton.Click += (s, e) => PlayPauseButtonClicked?.Invoke(s, e);

        _bufferingProgressBar = GetTemplateChild("NewBufferingProgressBar") as ProgressBar;

        _progressSlider = GetTemplateChild("NewProgressSlider") as Slider;
        _progressSlider.ManipulationMode = ManipulationModes.TranslateX;
        _progressSlider.ManipulationStarted += (s, e) =>
        {
            _isSliderDragged = true;
        };
        _progressSlider.ManipulationCompleted += (s, e) =>
        {
            SetBufferingState(false);
            ProgressSliderValueChanged?.Invoke(s, ((Slider)s).Value);
            _isSliderDragged = false;
        };

        // audio
        _audioStreamsComboBox = GetTemplateChild("NewAudioStreamsComboBox") as ComboBox;
        _audioStreamsComboBox.ItemsSource = AudioStreams;
        _audioStreamsComboBox.SelectionChanged += (s, e) =>
        {
            if (_audioStreamsComboBox.SelectedItem is NamedMediaSource namedMediaStream)
                AudioStreamChanged?.Invoke(this, namedMediaStream);
        };
        _audioStreamsComboBox.SelectedIndex = _audioStreamsComboBox.Items.Count - 1;

        // video
        _videoStreamsComboBox = GetTemplateChild("VideoStreamsComboBox") as ComboBox;
        _videoStreamsComboBox.ItemsSource = VideoStreams;
        _videoStreamsComboBox.SelectionChanged += (s, e) =>
        {
            if (_videoStreamsComboBox.SelectedItem is NamedMediaSource namedMediaStream)
                VideoStreamChanged?.Invoke(this, namedMediaStream);
        };
        _videoStreamsComboBox.SelectedIndex = Math.Max(_videoStreamsComboBox.Items.Count - 3, -1);

        // captions
        _ccSelectionButton = GetTemplateChild("CCSelectionButton") as AppBarToggleButton;

        _captionsComboBox = GetTemplateChild("CaptionsComboBox") as ComboBox;
        _captionsComboBox.ItemsSource = CaptionSources;
        _captionsComboBox.SelectedIndex = _captionsComboBox.Items.Count - 1;

        if (CaptionSources.Count <= 0)
        {
            _ccSelectionButton.IsEnabled = false;
            _captionsComboBox.IsEnabled = false;
        }

        _ccSelectionButton.Click += (s, e) =>
        {
            if (_ccSelectionButton.IsChecked != true)
                CaptionChanged?.Invoke(this, -1);
            else
                CaptionChanged?.Invoke(this, _captionsComboBox.SelectedIndex);
        };

        _captionsComboBox.SelectionChanged += (s, e) =>
        {
            CaptionChanged?.Invoke(this, _ccSelectionButton.IsChecked == true ? _captionsComboBox.SelectedIndex : -1);
        };

        base.OnApplyTemplate();
    }
}