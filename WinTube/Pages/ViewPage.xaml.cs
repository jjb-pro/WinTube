using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTube.ViewModels;
using YoutubeExplode.Videos;

namespace WinTube.Pages
{
    public sealed partial class ViewPage : Page
    {
        public ViewViewModel ViewModel { get; } = ((App)Application.Current).Container.GetService<ViewViewModel>();

        public ViewPage() => InitializeComponent();

        private bool isSliderGrabbed = false;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (null != e.Parameter && e.Parameter is VideoId videoId)
                await ViewModel.Watch(videoId);

            mediaControl.SetSources(ViewModel.AudioMediaSource, ViewModel.VideoMediaSource);

            //videoPlayerElement.MediaPlayer.Volume = 0f;

            ////slider.PointerEntered += (s, x) =>
            ////{
            ////    isSliderGrabbed = true;
            ////};
            ////slider.PointerReleased += (s, x) => isSliderGrabbed = false;
            ////slider.PointerCaptureLost += (s, x) => isSliderGrabbed = false; // Handles edge cases

            //_mediaTimelineController.PositionChanged += OnPositionChanged;

            //audioPlayerElement.MediaPlayer.CommandManager.IsEnabled = false;
            //audioPlayerElement.MediaPlayer.TimelineController = _mediaTimelineController;

            //videoPlayerElement.MediaPlayer.CommandManager.IsEnabled = false;
            //videoPlayerElement.MediaPlayer.TimelineController = _mediaTimelineController;

            //slider.Maximum = ViewModel.Duration.GetValueOrDefault().TotalSeconds;
            //slider.StepFrequency = 1;
        }

        private double _current;

        //private async void OnPositionChanged(MediaTimelineController sender, object args)
        //{
        //    if (isSliderGrabbed)
        //        return;

        //    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        _current = sender.Position.TotalSeconds;
        //        customMediaTransportControls.ProgressSlider.Value = _current; // (float)ViewModel.Duration.GetValueOrDefault().TotalSeconds;
        //    }).AsTask();
        //}

        //private readonly MediaTimelineController _mediaTimelineController = new MediaTimelineController();

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_mediaTimelineController.State == MediaTimelineControllerState.Running)
        //        _mediaTimelineController.Pause();
        //    else
        //        _mediaTimelineController.Resume();
        //}

        //private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        //{
        //   if (e.NewValue - e.OldValue > 5)
        //        _mediaTimelineController.Position = TimeSpan.FromSeconds(e.NewValue);
        //}

        //private void AppBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    videoPlayerElement.IsFullWindow = true;
        //}

        //private void customMediaTransportControls_PlayClicked(object sender, RoutedEventArgs e)
        //{
        //    if (_mediaTimelineController.State == MediaTimelineControllerState.Running)
        //        _mediaTimelineController.Pause();
        //    else
        //        _mediaTimelineController.Resume();
        //}
    }
}