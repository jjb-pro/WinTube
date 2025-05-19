using Mail.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinTube.Services;
using WinTube.ViewModels;

namespace WinTube
{
    sealed partial class App : Application
    {
        public IServiceProvider Container { get; private set; }

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        private static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<NavigationService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<SearchViewModel>();
            services.AddTransient<ViewViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Container = RegisterServices();

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);

                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}