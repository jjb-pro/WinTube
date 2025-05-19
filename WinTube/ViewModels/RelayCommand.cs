using System;
using System.Windows.Input;

namespace WinTube.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action action, Func<bool> canExecute = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _action();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}