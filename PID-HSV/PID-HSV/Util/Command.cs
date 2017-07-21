using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PID_HSV.Util
{
    public class Command : ICommand
    {
        private readonly Func<bool> _canExecute;

        private readonly Action _execute;

        public event EventHandler CanExecuteChanged;

        public Command(Action execute) : this(execute, () => true)
        {

        }

        public Command(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
