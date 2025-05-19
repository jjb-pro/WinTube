using Windows.Storage;

namespace WinTube.Services
{
    public class SettingsService
    {
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public Quality AudioQuality { get; set; }
        public Quality VideoQuality { get; set; }

        public uint SearchResultsLimit { get; set; }

        public uint RewindDuration { get; set; }
        public uint ForwardDuration { get; set; }

        public void Initialize()
        {
            object value;
            AudioQuality = localSettings.Values.TryGetValue("AudioQuality", out value) ? (Quality)value : Quality.Highest;
            VideoQuality = localSettings.Values.TryGetValue("VideoQuality", out value) ? (Quality)value : Quality.Highest;
            
            SearchResultsLimit = localSettings.Values.TryGetValue("SearchResultsLimit", out value) ? (uint)value : 10;

            RewindDuration = localSettings.Values.TryGetValue("RewindDuration", out value) ? (uint)value : 10;
            ForwardDuration = localSettings.Values.TryGetValue("ForwardDuration", out value) ? (uint)value : 30;
        }

        public void SaveSettings()
        {
            localSettings.Values["AudioQuality"] = AudioQuality;
            localSettings.Values["VideoQuality"] = VideoQuality;

            localSettings.Values["SearchResultsLimit"] = SearchResultsLimit;

            localSettings.Values["RewindDuration"] = RewindDuration;
            localSettings.Values["ForwardDuration"] = ForwardDuration;
        }
    }

    public enum Quality
    {
        Lowest,
        Highest
    }
}