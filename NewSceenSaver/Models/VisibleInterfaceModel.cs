using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.ObjectModel;

using NewScreenSaver.RFIDModul;
using System.Windows;

namespace NewScreenSaver.Models
{
    public class VisibleInterfaceModel :ViewModelBase
    {

        RFIDScanBase _rfid;

        public bool IsOpen
        {
            get
            {
                if (_rfid != null)
                    return _rfid.IsOpen;
                else
                    return false;
            }
        }

        ObservableCollection<ModelUser> _users = new ObservableCollection<ModelUser>();

        /// <summary>
        /// информация по станции
        /// </summary>
        public ObservableCollection<ModelUser> Users
        {
            get
            {
                return _users;
            }

            set
            {
                _users = value;
            }
        }

        private ModelUser _selectedItem = new ModelUser();
        public ModelUser SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged("SelectedItem"); }
        }

        private int _selectedIndex  = -1;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; OnPropertyChanged("SelectedIndex"); }
        }

        private string _login = string.Empty;
        public string Login
        {
            get
            {
                if (_selectedIndex == -1)
                    return _login;
                else
                {
                    OnPropertyChanged("ToolTipUser");
                    OnPropertyChanged("ToolTipUserVisible");
                    return _selectedItem.Login;
                }
            }
            set
            {
                _login = value;
                OnPropertyChanged("Login");
                OnPropertyChanged("ToolTipUser");
                OnPropertyChanged("ToolTipUserVisible");
            }

        }

        public string ToolTipUser
        {
            get
            {
                if (_selectedIndex == -1)
                    return null;
                else
                    return _selectedItem.UserName;
            }
        }

        public Visibility ToolTipUserVisible
        {
            get
            {
                if (string.IsNullOrEmpty(ToolTipUser))
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }

        //private string 

        //public string Login
        //{
        //    get
        //    {
        //        return rfid.IsOpen;
        //    }
        //}


        public void Initialization(RFIDScanBase rfid, Dispatcher dispatcher)
        {
            this._rfid = rfid;
            rfid.ConnectDevice +=
                (status) => { OnPropertyChanged("IsOpen"); };
            //dispatcher.Invoke(DispatcherPriority.Render,
            //                                          new Action(() =>
            //                                          {

            //                                          }));
        }
    }
}
