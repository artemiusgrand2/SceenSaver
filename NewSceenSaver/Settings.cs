using System;
using System.Windows;
using System.Windows.Input;

namespace NewScreenSaver
{
    public class NotifyIconViewModel
    {


        //public string Login
        //{
        //    get
        //    {
        //        return (Application.Current.MainWindow as MainWindow).VisibleInterfaceModel.Login;
        //    }
        //}

        ///// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {


            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        if(Application.Current.MainWindow != null)
                            (Application.Current.MainWindow as MainWindow).ProcessAuthenticateTimerProc(null, null);
                    }
                };
            }
        }



    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
