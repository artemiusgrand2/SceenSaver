using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Ports;
using System.Timers;
using Authentificator;
using Authentificator.Enums;

using NewScreenSaver.Models;
using NewScreenSaver.OtherScreens;
using NewScreenSaver.Enums;

namespace NewScreenSaver.RFIDModul
{

    public delegate void StatusAuthorization(bool status);

    public delegate void StatusConnectComPort(bool status);

    public abstract class RFIDScanBase 
    {
        #region Переменные и свойства
        /// <summary>
        /// закрыт ли порт
        /// </summary>
        private bool _isOpen;

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            protected set
            {
                _isOpen = value;
                if (ConnectDevice != null)
                    ConnectDevice(_isOpen);
            }
        }

        private bool _isAuthorization;

        public bool IsAuthorization
        {
            get
            {
                return _isAuthorization;
            }
            protected set
            {
                if(_isAuthorization != value)
                {
                    _isAuthorization = value;
                    if (Authorization != null)
                        Authorization(_isAuthorization);
                }
            }
        }
        /// <summary>
        /// таймер обработки ком порта
        /// </summary>
        private Timer _timerScan;
        /// <summary>
        /// класс проверки кода rfid
        /// </summary>
        protected Authentificators _auth;
        /// <summary>
        /// первое ли это подключение
        /// </summary>
        protected bool _isfirst_connect = true;
        /// <summary>
        /// вид считывателя
        /// </summary>
        protected ViewReader _viewReader;

        /// <summary>
        ///есть ли связь с Com port
        /// </summary>
        public event StatusConnectComPort ConnectDevice;
        /// <summary>
        /// событие авторизации
        /// </summary>
        public event StatusAuthorization Authorization;

        #endregion

        public RFIDScanBase(Authentificators auth)
        {
            this._auth = auth;
            _timerScan = new Timer(100);
            _timerScan.Elapsed+=timerCom_Elapsed;
        }

        public void Start()
        {
            _timerScan.Start();
        }

        public void Stop()
        {
            _timerScan.Stop();
        }

        protected abstract void Scan();

        private void timerCom_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerScan.Stop();
            Scan();
            _timerScan.Start();
        }

    }
}
