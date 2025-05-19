using System.Windows.Input;
using WinTube.Services;

namespace WinTube.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly SettingsService _settingsService;

        public ICommand SaveCommand { get; set; }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void Initialize() => _settingsService.Initialize();


    }
}