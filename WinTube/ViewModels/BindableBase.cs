using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinTube.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T originalValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(originalValue, newValue))
            {
                originalValue = newValue;
                OnPropertyChanged(propertyName);

                return true;
            }

            return false;
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}