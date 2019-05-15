using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common.Base
{
    public class MyCommand : ICommand
    {
        private Action<object> action;

        public bool CanExecute(object parameter)
        {
            //if (CanExecuteChanged != null)
            //{
            //    CanExecuteChanged(this, new EventArgs());
            //}
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.action != null)
            {
                this.action(parameter);
            }
        }

        public MyCommand(Action<object> action)
        {
            this.action = action;
        }
    }
}
