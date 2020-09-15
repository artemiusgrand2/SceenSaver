using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Ports;
using System.Timers;
using Authentificator;

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

        protected SerialPort _serialPort;
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
                if (ConnectComPort != null)
                    ConnectComPort(_isOpen);
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

        protected IList<string> bufferNameComPorts = new List<string>();
        /// <summary>
        ///есть ли связь с Com port
        /// </summary>
        public event StatusConnectComPort ConnectComPort;
        /// <summary>
        /// событие авторизации
        /// </summary>
        public event StatusAuthorization Authorization;

        #endregion

        public RFIDScanBase(int baudRate, Authentificators auth)
        {
            this._auth = auth;
            _serialPort = new SerialPort() { ReadTimeout = 500, BaudRate = baudRate};
            bufferNameComPorts.Add(_serialPort.PortName);
            _timerScan = new Timer(100);
            _timerScan.Elapsed+=timerCom_Elapsed;
        }

        public void Start()
        {
            _timerScan.Start();
        }

        public void Stop()
        {
            try
            {
                _serialPort.Close();
            }
            catch { };
            _timerScan.Stop();
        }

        protected void RenameComPort()
        {
            var newName = SerialPort.GetPortNames().Where(x => !bufferNameComPorts.Contains(x)).FirstOrDefault();
            if (string.IsNullOrEmpty(newName))
            {
                bufferNameComPorts.Clear();
              //  RenameComPort();
            }
            else
            {
                _serialPort.PortName = newName;
                bufferNameComPorts.Add(newName);
            }
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
