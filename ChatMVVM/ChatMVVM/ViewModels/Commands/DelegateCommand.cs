using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatMVVM.ViewModels.Commands
{
    class DelegateCommand : ICommand
    {
        private Action<object> execute;
        private Func<object,bool> CanExecute;
       // public ChatViewModel ViewModel { get; set; }
        public DelegateCommand(Action<object> execute, Func<object, bool> CanExecute = null)
        {
            this.execute = execute;
            this.CanExecute = CanExecute;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute == null || CanExecute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
