using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Ports;
using System.Timers;
using Authentificator;
using Authentificator.Enums;

namespace NewScreenSaver.RFIDModul
{
    public abstract class RFIDScanSerialBase : RFIDScanBase
    {
        #region Переменные и свойства

        protected SerialPort _serialPort;

        private bool _isAuthorization;

        protected IList<string> bufferNameComPorts = new List<string>();

        #endregion

        public RFIDScanSerialBase(int baudRate, Authentificators auth):base(auth)
        {
            this._auth = auth;
            _serialPort = new SerialPort() { ReadTimeout = 500, BaudRate = baudRate };
            bufferNameComPorts.Add(_serialPort.PortName);
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

        protected bool CheckNameComPorts(string portName)
        {
            return SerialPort.GetPortNames().Contains(portName);
        }

        public new void Start()
        {
            base.Start();
        }

        public new void Stop()
        {
            base.Stop();
            try
            {
                _serialPort.Close();
            }
            catch { };
        }
    }
}
