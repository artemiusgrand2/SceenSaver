using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NewScreenSaver.Models
{
    public class ModelUser : ViewModelBase
    {
        #region Переменные

        #endregion

        public string UserName { get; private set; }

        public string Login { get; private set; }

        private double _widthUser = 100;
        public double WidthUser
        {
            get
            {
                return _widthUser;
            }
            set
            {
                _widthUser = value;
                OnPropertyChanged("WidthUser");
            }

        }


        public ModelUser(string login, string userName)
        {
            Login = login;
            UserName = userName;
        }

        public ModelUser() { }

    }
}
