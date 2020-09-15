using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace NewSceenSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Performing protection from repeated loading of the Application
            // --------------------------------------------------------------
            string _strTmp = "Transparent Screen Saver - KTC";
            // if parameter requeatInitialOwnership is true then it means that the calling
            // thread is given initial ownership (монопольное использование) of the mutex
            bool _requestInitialOwnership = true;
            // this param is passed uninitialized
            bool _mutexWasCreated = false;

            try
            {
                mutex = new Mutex(_requestInitialOwnership, _strTmp, out _mutexWasCreated);
            }
            catch (Exception _ex)
            {
                MessageBox.Show(_ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            if (_mutexWasCreated == false)
            {
                MessageBox.Show("Данная программа уже запущена!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            // ---------------------------
            //if (mutex != null)
            //    mutex.ReleaseMutex();
        }
    }
}
