using DependencyPropertyGenerator;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinTube.Model;

namespace WinTube.Controls;

[DependencyProperty<ObservableCollection<NamedMediaSource>>("AudioStreams", DefaultValueExpression = "[]")]
[DependencyProperty<ObservableCollection<NamedMediaSource>>("VideoStreams", DefaultValueExpression = "[]")]
[DependencyProperty<ObservableCollection<NamedMediaSource>>("CaptionSources", DefaultValueExpression = "[]")]
public sealed partial class CustomMediaTransportControls : MediaTransportControls
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

    public event EventHandler<NamedMediaSource> AudioStreamChanged;
    public event EventHandler<NamedMediaSource> VideoStreamChanged;
    public event EventHandler<int> CaptionChanged;

    public CustomMediaTransportControls() => DefaultStyleKey = typeof(CustomMediaTransportControls);

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

        InitialiezAudioControls();
        InitializeVideoControls();
        InitializeCaptionControls();

        base.OnApplyTemplate();
    }

    private void InitialiezAudioControls()
    {
        _audioStreamsComboBox = GetTemplateChild("NewAudioStreamsComboBox") as ComboBox;
        _audioStreamsComboBox.ItemsSource = AudioStreams;
        _audioStreamsComboBox.SelectionChanged += (s, e) =>
        {
            if (_audioStreamsComboBox.SelectedItem is NamedMediaSource namedMediaStream)
                AudioStreamChanged?.Invoke(this, namedMediaStream);
        };
        
        AudioStreams.CollectionChanged += OnAudioStreamsChanged;
    }

    private void OnAudioStreamsChanged(object sender, NotifyCollectionChangedEventArgs e) => _audioStreamsComboBox.SelectedIndex = _audioStreamsComboBox.Items.Count - 1;

    private void InitializeVideoControls()
    {
        _videoStreamsComboBox = GetTemplateChild("VideoStreamsComboBox") as ComboBox;
        _videoStreamsComboBox.ItemsSource = VideoStreams;
        _videoStreamsComboBox.SelectionChanged += (s, e) =>
        {
            if (_videoStreamsComboBox.SelectedItem is NamedMediaSource namedMediaStream)
                VideoStreamChanged?.Invoke(this, namedMediaStream);
        };

        VideoStreams.CollectionChanged += OnVideoStreamsChanged;
    }

    private void OnVideoStreamsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var lastValid = VideoStreams.LastOrDefault(s => s.ContainerType == "mp4");
        _videoStreamsComboBox.SelectedIndex = lastValid != null ? VideoStreams.IndexOf(lastValid) : _videoStreamsComboBox.Items.Count - 1;
    }

    private void InitializeCaptionControls()
    {
        _ccSelectionButton = GetTemplateChild("CCSelectionButton") as AppBarToggleButton;
        _ccSelectionButton.IsEnabled = false;
        _ccSelectionButton.Click += (s, e) =>
        {
            if (_ccSelectionButton.IsChecked != true)
                CaptionChanged?.Invoke(this, -1);
            else
                CaptionChanged?.Invoke(this, _captionsComboBox.SelectedIndex);
        };

        _captionsComboBox = GetTemplateChild("CaptionsComboBox") as ComboBox;
        _captionsComboBox.IsEnabled = false;
        _captionsComboBox.ItemsSource = CaptionSources;
        _captionsComboBox.SelectionChanged += (s, e) =>
        {
            CaptionChanged?.Invoke(this, _ccSelectionButton.IsChecked == true ? _captionsComboBox.SelectedIndex : -1);
        };

        CaptionSources.CollectionChanged += OnCaptionSourcesChanged;
    }

    private void OnCaptionSourcesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _captionsComboBox.SelectedIndex = _captionsComboBox.Items.Count - 1;

        var enable = CaptionSources.Count > 0;
        _ccSelectionButton.IsEnabled = enable;
        _captionsComboBox.IsEnabled = enable;
    }
}